using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using cookwise.Models;

namespace cookwise.Services;

public class RecipeService
{
    private static RecipeService? _instance;
    public static RecipeService Instance => _instance ??= new RecipeService();

    private readonly HttpClient _httpClient;
    
    // TheMealDB API (免费公开 API，无需密钥)
    private readonly string _apiBaseUrl = "https://www.themealdb.com/api/json/v1/1";
    
    private List<Recipe> _recipes = new();
    private List<Recipe> _apiRecipes = new(); // 从 API 获取的菜谱
    private readonly List<UserNote> _userNotes = new();
    private readonly FlavorProfile _flavorProfile = new();
    private bool _isInitialized = false;
    private bool _apiInitialized = false;
    
    // 加载状态
    private bool _isLoading = false;
    public bool IsLoading => _isLoading;
    
    // 错误信息
    private string _lastError = string.Empty;
    public string LastError => _lastError;
    
    // 数据来源标识
    private string _dataSource = "local";
    public string DataSource => _dataSource;
    
    // 笔记持久化文件路径
    private readonly string _notesFilePath;
    private readonly string _flavorProfileFilePath;

    private RecipeService()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        // 设置笔记文件路径 (在应用数据目录)
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _notesFilePath = Path.Combine(appDataPath, "cookwise_notes.json");
        _flavorProfileFilePath = Path.Combine(appDataPath, "cookwise_flavor.json");
        
        // 初始化本地示例数据（作为离线备选）
        InitializeLocalSampleData();
    }

    private async Task EnsureInitializedAsync()
    {
        if (_isInitialized) return;
        
        _isLoading = true;
        _dataSource = "loading";
        OnPropertyChanged?.Invoke(nameof(IsLoading));
        
        // 从文件加载笔记
        await LoadNotesFromFileAsync();
        await LoadFlavorProfileFromFileAsync();
        
        // 尝试从 API 加载数据
        try
        {
            await LoadRecipesFromApiAsync();
            _dataSource = "api";
            _apiInitialized = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load from API: {ex.Message}");
            _lastError = ex.Message;
            _dataSource = "local";
            
            // 使用本地示例数据作为回退
            _recipes = _recipes.Concat(GetLocalSampleRecipes()).DistinctBy(r => r.Id).ToList();
        }
        
        _isInitialized = true;
        _isLoading = false;
        OnPropertyChanged?.Invoke(nameof(IsLoading));
        OnPropertyChanged?.Invoke(nameof(DataSource));
    }
    
    // 状态变更通知
    public event Action<string>? OnPropertyChanged;

    /// <summary>
    /// 从 TheMealDB API 加载菜谱
    /// </summary>
    private async Task LoadRecipesFromApiAsync()
    {
        // 获取多个随机菜谱
        var tasks = new List<Task<List<Recipe>>>();
        
        // 同时请求 8 个随机菜谱
        for (int i = 0; i < 8; i++)
        {
            tasks.Add(FetchRandomRecipeAsync());
        }
        
        var results = await Task.WhenAll(tasks);
        _apiRecipes = results.SelectMany(r => r).ToList();
        
        if (_apiRecipes.Any())
        {
            _recipes = _apiRecipes.ToList();
        }
    }
    
    private async Task<List<Recipe>> FetchRandomRecipeAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<MealDbResponse>($"{_apiBaseUrl}/random.php");
            if (response?.Meals != null && response.Meals.Any())
            {
                return response.Meals.Select(MapMealToRecipe).ToList();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FetchRandomRecipeAsync error: {ex.Message}");
        }
        return new List<Recipe>();
    }

    /// <summary>
    /// 将 TheMealDB 的数据映射到 Recipe 模型
    /// </summary>
    private Recipe MapMealToRecipe(MealDbMeal meal)
    {
        var recipe = new Recipe
        {
            Id = $"api_{meal.IdMeal}",
            Name = meal.StrMeal ?? "Unknown",
            Description = meal.StrInstructions ?? "",
            ImageUrl = meal.StrMealThumb ?? "https://pic1.imgdb.cn/item/67e8a7a088c538a9b5bc4b5a.png",
            Servings = 4,
            EstimatedCost = new Random().Next(5, 25),
            CategoryTags = new List<string>(),
            Ingredients = new List<Ingredient>(),
            Steps = new List<CookingStep>(),
            CookingTime = $"{new Random().Next(15, 60)} mins",
            Calories = new Random().Next(200, 600),
            IsCustom = false
        };
        
        // 添加分类标签
        if (!string.IsNullOrEmpty(meal.StrCategory))
            recipe.CategoryTags.Add(meal.StrCategory);
        if (!string.IsNullOrEmpty(meal.StrArea))
            recipe.CategoryTags.Add(meal.StrArea);
        
        // 解析食材和用量
        for (int i = 1; i <= 20; i++)
        {
            var ingredient = GetPropertyValue(meal, $"StrIngredient{i}") as string;
            var measure = GetPropertyValue(meal, $"StrMeasure{i}") as string;
            
            if (!string.IsNullOrWhiteSpace(ingredient))
            {
                recipe.Ingredients.Add(new Ingredient
                {
                    Name = ingredient,
                    Grams = ParseMeasureToGrams(measure),
                    Category = CategorizeIngredient(ingredient),
                    IconUrl = GetIngredientEmoji(ingredient)
                });
            }
        }
        
        // 将说明文本分段作为步骤
        if (!string.IsNullOrEmpty(meal.StrInstructions))
        {
            var steps = meal.StrInstructions
                .Split(new[] { "\r\n", "\n", ". " }, StringSplitOptions.RemoveEmptyEntries)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select((s, idx) => new CookingStep
                {
                    StepNumber = idx + 1,
                    Description = s.TrimEnd('.'),
                    DurationMinutes = EstimateDuration(s)
                })
                .ToList();
            
            recipe.Steps = steps.Any() ? steps : new List<CookingStep>
            {
                new() { StepNumber = 1, Description = meal.StrInstructions }
            };
        }
        
        return recipe;
    }
    
    private string? GetPropertyValue(object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(propertyName);
        return property?.GetValue(obj)?.ToString();
    }
    
    private double ParseMeasureToGrams(string? measure)
    {
        if (string.IsNullOrWhiteSpace(measure)) return 0;
        
        // 提取数字部分
        var digits = new string(measure.Where(char.IsDigit).ToArray());
        if (double.TryParse(digits, out var amount))
        {
            // 简单估算：如果是重量单位直接用，否则按体积估算
            if (measure.Contains("kg", StringComparison.OrdinalIgnoreCase)) return amount * 1000;
            if (measure.Contains("g", StringComparison.OrdinalIgnoreCase)) return amount;
            if (measure.Contains("lb", StringComparison.OrdinalIgnoreCase)) return amount * 454;
            if (measure.Contains("oz", StringComparison.OrdinalIgnoreCase)) return amount * 28;
            if (measure.Contains("cup", StringComparison.OrdinalIgnoreCase)) return amount * 240;
            if (measure.Contains("tbsp", StringComparison.OrdinalIgnoreCase)) return amount * 15;
            if (measure.Contains("tsp", StringComparison.OrdinalIgnoreCase)) return amount * 5;
            
            // 默认按克估算
            return amount;
        }
        return 0;
    }
    
    private string CategorizeIngredient(string ingredient)
    {
        var lower = ingredient.ToLower();
        if (lower.Contains("chicken") || lower.Contains("beef") || lower.Contains("pork") 
            || lower.Contains("鱼") || lower.Contains("肉") || lower.Contains("鸡") || lower.Contains("猪"))
            return "肉类";
        if (lower.Contains("vegetable") || lower.Contains("tomato") || lower.Contains("onion")
            || lower.Contains("蔬菜") || lower.Contains("番茄") || lower.Contains("葱"))
            return "蔬菜";
        if (lower.Contains("oil") || lower.Contains("salt") || lower.Contains("pepper")
            || lower.Contains("酱") || lower.Contains("盐") || lower.Contains("油"))
            return "调料";
        if (lower.Contains("egg") || lower.Contains("蛋"))
            return "蛋白质";
        if (lower.Contains("milk") || lower.Contains("cheese") || lower.Contains("butter")
            || lower.Contains("奶") || lower.Contains("酪"))
            return "乳制品";
        return "其他";
    }
    
    private string GetIngredientEmoji(string ingredient)
    {
        var lower = ingredient.ToLower();
        if (lower.Contains("chicken") || lower.Contains("鸡")) return "🍗";
        if (lower.Contains("beef") || lower.Contains("牛")) return "🥩";
        if (lower.Contains("pork") || lower.Contains("猪")) return "🥓";
        if (lower.Contains("fish") || lower.Contains("鱼")) return "🐟";
        if (lower.Contains("egg") || lower.Contains("蛋")) return "🥚";
        if (lower.Contains("tomato") || lower.Contains("番茄")) return "🍅";
        if (lower.Contains("onion") || lower.Contains("葱")) return "🧅";
        if (lower.Contains("garlic") || lower.Contains("蒜")) return "🧄";
        if (lower.Contains("carrot") || lower.Contains("胡萝卜")) return "🥕";
        if (lower.Contains("potato") || lower.Contains("土豆")) return "🥔";
        if (lower.Contains("rice") || lower.Contains("米")) return "🍚";
        if (lower.Contains("noodle") || lower.Contains("面")) return "🍜";
        return "🥬";
    }
    
    private int? EstimateDuration(string instruction)
    {
        var lower = instruction.ToLower();
        if (lower.Contains("boil") || lower.Contains("simmer")) return new Random().Next(5, 20);
        if (lower.Contains("fry") || lower.Contains("sauté")) return new Random().Next(3, 10);
        if (lower.Contains("bake") || lower.Contains("roast")) return new Random().Next(20, 60);
        if (lower.Contains("marinat")) return new Random().Next(10, 30);
        return null;
    }

    /// <summary>
    /// 初始化本地示例数据（离线备选）
    /// </summary>
    private void InitializeLocalSampleData()
    {
        _recipes = GetLocalSampleRecipes();
    }
    
    private List<Recipe> GetLocalSampleRecipes()
    {
        return new List<Recipe>
        {
            // 番茄炒蛋
            new Recipe
            {
                Id = "local_001",
                Name = "番茄炒蛋",
                ImageUrl = "https://pic1.imgdb.cn/item/67e8a7a088c538a9b5bc4b5a.png",
                CookingTime = "15 mins",
                Calories = 250,
                Description = "经典家常菜，酸甜可口，简单易学",
                Servings = 2,
                EstimatedCost = 3.50,
                CategoryTags = new List<string> { "快手菜", "家常菜", "素菜" },
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "番茄", Grams = 200, Category = "蔬菜" },
                    new() { Name = "鸡蛋", Grams = 100, Category = "蛋白质" },
                    new() { Name = "盐", Grams = 3, Category = "调料" },
                    new() { Name = "糖", Grams = 10, Category = "调料" },
                    new() { Name = "食用油", Grams = 15, Category = "调料" }
                },
                Steps = new List<CookingStep>
                {
                    new() { StepNumber = 1, Description = "番茄洗净切块，鸡蛋打散备用", DurationMinutes = 2 },
                    new() { StepNumber = 2, Description = "热锅倒油，油热后倒入鸡蛋液", DurationMinutes = 1, HeatLevel = "中火" },
                    new() { StepNumber = 3, Description = "鸡蛋凝固后盛出备用", DurationMinutes = 1 },
                    new() { StepNumber = 4, Description = "放入番茄翻炒至出汁", DurationMinutes = 2 },
                    new() { StepNumber = 5, Description = "加入炒好的鸡蛋，放盐和糖调味", DurationMinutes = 1 }
                }
            },
            // 红烧肉
            new Recipe
            {
                Id = "local_002",
                Name = "Braised Pork",
                Description = "肥而不腻，入口即化，下饭神器",
                Servings = 4,
                EstimatedCost = 25.00,
                ImageUrl = "https://pic1.imgdb.cn/item/67e8a7a088c538a9b5bc4b5b.png",
                CookingTime = "60 mins",
                Calories = 450,
                CategoryTags = new List<string> { "家常菜", "硬菜", "下饭菜" },
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "Pork belly", Grams = 500, Category = "肉类" },
                    new() { Name = "Soy sauce", Grams = 30, Category = "调料" },
                    new() { Name = "Dark soy sauce", Grams = 15, Category = "调料" },
                    new() { Name = "Sugar", Grams = 30, Category = "调料" },
                    new() { Name = "Cooking wine", Grams = 20, Category = "调料" }
                },
                Steps = new List<CookingStep>
                {
                    new() { StepNumber = 1, Description = "Cut pork belly into chunks, blanch in cold water with cooking wine", DurationMinutes = 10, HeatLevel = "大火" },
                    new() { StepNumber = 2, Description = "Add oil to pan, add sugar and stir-fry until caramelized", DurationMinutes = 3, HeatLevel = "中小火" },
                    new() { StepNumber = 3, Description = "Add pork belly and stir-fry until colored", DurationMinutes = 2, HeatLevel = "中火" },
                    new() { StepNumber = 4, Description = "Add soy sauce, dark soy sauce, cooking wine and spices", DurationMinutes = 1 },
                    new() { StepNumber = 5, Description = "Add boiling water to cover meat, simmer over low heat for 1 hour", DurationMinutes = 60, HeatLevel = "小火" },
                    new() { StepNumber = 6, Description = "Reduce sauce over high heat until thickened", DurationMinutes = 10, HeatLevel = "大火" }
                }
            },
            // 宫保鸡丁
            new Recipe
            {
                Id = "local_003",
                Name = "宫保鸡丁",
                ImageUrl = "https://pic1.imgdb.cn/item/67e8a7a188c538a9b5bc4b5c.png",
                CookingTime = "25 mins",
                Calories = 400,
                Description = "麻辣鲜香，鸡肉嫩滑，花生脆爽",
                Servings = 2,
                EstimatedCost = 15.00,
                CategoryTags = new List<string> { "川菜", "辣味", "下饭菜" },
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "鸡胸肉", Grams = 200, Category = "肉类" },
                    new() { Name = "花生米", Grams = 50, Category = "坚果" },
                    new() { Name = "干辣椒", Grams = 10, Category = "调料" },
                    new() { Name = "花椒", Grams = 3, Category = "调料" },
                    new() { Name = "葱段", Grams = 20, Category = "蔬菜" }
                },
                Steps = new List<CookingStep>
                {
                    new() { StepNumber = 1, Description = "鸡胸肉切丁，用盐、淀粉腌制10分钟", DurationMinutes = 10 },
                    new() { StepNumber = 2, Description = "调好酱汁：生抽+醋+糖+水淀粉", DurationMinutes = 1 },
                    new() { StepNumber = 3, Description = "热锅凉油下花生米炒至金黄酥脆", DurationMinutes = 2, HeatLevel = "小火" },
                    new() { StepNumber = 4, Description = "下鸡丁炒至变色盛出备用", DurationMinutes = 2, HeatLevel = "中火" },
                    new() { StepNumber = 5, Description = "炒香干辣椒和花椒", DurationMinutes = 1, HeatLevel = "小火" },
                    new() { StepNumber = 6, Description = "放入鸡丁和酱汁快速翻炒", DurationMinutes = 1, HeatLevel = "大火" }
                }
            },
            // 麻婆豆腐
            new Recipe
            {
                Id = "local_004",
                Name = "麻婆豆腐",
                ImageUrl = "https://pic1.imgdb.cn/item/67e8a7a188c538a9b5bc4b5d.png",
                CookingTime = "20 mins",
                Calories = 320,
                Description = "麻辣鲜香，豆腐嫩滑，川菜经典",
                Servings = 2,
                EstimatedCost = 8.00,
                CategoryTags = new List<string> { "川菜", "辣味", "素菜" },
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "嫩豆腐", Grams = 300, Category = "豆制品" },
                    new() { Name = "猪肉末", Grams = 100, Category = "肉类" },
                    new() { Name = "郫县豆瓣酱", Grams = 30, Category = "调料" },
                    new() { Name = "花椒粉", Grams = 3, Category = "调料" }
                },
                Steps = new List<CookingStep>
                {
                    new() { StepNumber = 1, Description = "豆腐切小块，用盐水焯水去腥", DurationMinutes = 2, HeatLevel = "大火" },
                    new() { StepNumber = 2, Description = "热锅下油，炒香姜蒜末和豆瓣酱", DurationMinutes = 1, HeatLevel = "中小火" },
                    new() { StepNumber = 3, Description = "放入肉末炒散至变色", DurationMinutes = 2, HeatLevel = "中火" },
                    new() { StepNumber = 4, Description = "加适量水，放入豆腐块", DurationMinutes = 1 },
                    new() { StepNumber = 5, Description = "加入生抽，中火煮3分钟", DurationMinutes = 3, HeatLevel = "中火" }
                }
            },
            // 可乐鸡翅
            new Recipe
            {
                Id = "local_005",
                Name = "可乐鸡翅",
                ImageUrl = "https://pic1.imgdb.cn/item/67e8a7a388c538a9b5bc4b60.png",
                CookingTime = "30 mins",
                Calories = 450,
                Description = "香甜软嫩，老少皆爱的懒人菜",
                Servings = 2,
                EstimatedCost = 12.00,
                CategoryTags = new List<string> { "懒人菜", "家常菜", "肉类" },
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "鸡翅中", Grams = 300, Category = "肉类" },
                    new() { Name = "可乐", Grams = 330, Category = "饮料" },
                    new() { Name = "生抽", Grams = 20, Category = "调料" },
                    new() { Name = "老抽", Grams = 10, Category = "调料" }
                },
                Steps = new List<CookingStep>
                {
                    new() { StepNumber = 1, Description = "鸡翅洗净，表面划两刀", DurationMinutes = 2 },
                    new() { StepNumber = 2, Description = "冷水下锅，加料酒姜片焯水", DurationMinutes = 5, HeatLevel = "大火" },
                    new() { StepNumber = 3, Description = "热锅下油，放入鸡翅煎至两面金黄", DurationMinutes = 5, HeatLevel = "中火" },
                    new() { StepNumber = 4, Description = "倒入可乐（没过鸡翅）、生抽、老抽", DurationMinutes = 1 },
                    new() { StepNumber = 5, Description = "大火烧开后转中小火炖15分钟", DurationMinutes = 15, HeatLevel = "中小火" }
                }
            }
        };
    }

    public async Task<List<Recipe>> GetAllRecipesAsync()
    {
        await EnsureInitializedAsync();
        return _recipes.ToList();
    }

    public async Task<Recipe?> GetRecipeByIdAsync(string id)
    {
        await EnsureInitializedAsync();
        
        // 先查找本地
        var recipe = _recipes.FirstOrDefault(r => r.Id == id);
        if (recipe != null) return recipe;
        
        // 如果是 API ID，尝试从 TheMealDB 获取详情
        if (id.StartsWith("api_"))
        {
            var mealId = id.Replace("api_", "");
            try
            {
                var response = await _httpClient.GetFromJsonAsync<MealDbResponse>($"{_apiBaseUrl}/lookup.php?i={mealId}");
                if (response?.Meals != null && response.Meals.Any())
                {
                    return MapMealToRecipe(response.Meals.First());
                }
            }
            catch { }
        }
        
        return null;
    }

    /// <summary>
    /// 通过 TheMealDB API 搜索菜谱
    /// </summary>
    public async Task<List<Recipe>> SearchRecipesAsync(string keyword)
    {
        await EnsureInitializedAsync();
        
        if (string.IsNullOrWhiteSpace(keyword))
            return _recipes.ToList();

        // 尝试从 API 搜索
        try
        {
            _isLoading = true;
            OnPropertyChanged?.Invoke(nameof(IsLoading));
            
            var response = await _httpClient.GetFromJsonAsync<MealDbResponse>($"{_apiBaseUrl}/search.php?s={Uri.EscapeDataString(keyword)}");
            if (response?.Meals != null && response.Meals.Any())
            {
                var apiResults = response.Meals.Select(MapMealToRecipe).ToList();
                
                // 合并本地搜索结果
                var localResults = _recipes.Where(r =>
                    r.Name.ToLower().Contains(keyword.ToLower()) ||
                    r.Description.ToLower().Contains(keyword.ToLower()) ||
                    r.CategoryTags.Any(t => t.ToLower().Contains(keyword.ToLower()))
                ).ToList();
                
                _dataSource = "api";
                OnPropertyChanged?.Invoke(nameof(DataSource));
                
                return apiResults.Concat(localResults).DistinctBy(r => r.Id).ToList();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API search failed: {ex.Message}");
            _lastError = ex.Message;
        }
        finally
        {
            _isLoading = false;
            OnPropertyChanged?.Invoke(nameof(IsLoading));
        }

        // 回退到本地搜索
        _dataSource = "local";
        OnPropertyChanged?.Invoke(nameof(DataSource));
        
        var lowerKeyword = keyword.ToLower();
        return _recipes.Where(r =>
            r.Name.ToLower().Contains(lowerKeyword) ||
            r.Description.ToLower().Contains(lowerKeyword) ||
            r.CategoryTags.Any(t => t.ToLower().Contains(lowerKeyword)) ||
            r.Ingredients.Any(i => i.Name.ToLower().Contains(lowerKeyword))
        ).ToList();
    }

    /// <summary>
    /// 获取随机菜谱（展示 API 能力）
    /// </summary>
    public async Task<List<Recipe>> GetRandomRecipesAsync(int count = 3)
    {
        var recipes = new List<Recipe>();
        
        try
        {
            _isLoading = true;
            _dataSource = "loading";
            OnPropertyChanged?.Invoke(nameof(IsLoading));
            OnPropertyChanged?.Invoke(nameof(DataSource));
            
            for (int i = 0; i < count; i++)
            {
                var result = await FetchRandomRecipeAsync();
                recipes.AddRange(result);
            }
            
            // 将获取的菜谱添加到内部列表中，以便后续查找
            foreach (var recipe in recipes)
            {
                if (!_recipes.Any(r => r.Id == recipe.Id))
                {
                    _recipes.Add(recipe);
                }
            }
            
            _dataSource = "api";
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
            _dataSource = "local";
            
            // 回退到随机本地菜谱
            var random = new Random();
            recipes = _recipes.OrderBy(x => random.Next()).Take(count).ToList();
        }
        finally
        {
            _isLoading = false;
            OnPropertyChanged?.Invoke(nameof(IsLoading));
            OnPropertyChanged?.Invoke(nameof(DataSource));
        }
        
        return recipes;
    }

    public async Task<List<SearchResult>> SearchByIngredientsAsync(List<string> availableIngredients)
    {
        await EnsureInitializedAsync();
        
        if (availableIngredients == null || !availableIngredients.Any())
            return new List<SearchResult>();

        var results = new List<SearchResult>();
        var lowerIngredients = availableIngredients.Select(i => i.ToLower()).ToList();

        foreach (var recipe in _recipes)
        {
            var recipeIngredientNames = recipe.Ingredients.Select(i => i.Name.ToLower()).ToList();
            var matchedIngredients = recipeIngredientNames
                .Where(r => lowerIngredients.Any(l => r.Contains(l) || l.Contains(r)))
                .ToList();

            var missingIngredients = recipe.Ingredients
                .Where(i => !lowerIngredients.Any(l => i.Name.ToLower().Contains(l) || l.Contains(i.Name.ToLower())))
                .ToList();

            var score = matchedIngredients.Count * 10 - missingIngredients.Count * 5;

            if (matchedIngredients.Any())
            {
                results.Add(new SearchResult
                {
                    Recipe = recipe,
                    MissingIngredients = missingIngredients,
                    MatchScore = score
                });
            }
        }

        results = results.OrderByDescending(r => r.MatchScore).ToList();
        return results;
    }

    public Recipe ScaleRecipe(Recipe recipe, int newServings)
    {
        var scaledRecipe = new Recipe
        {
            Id = recipe.Id,
            Name = recipe.Name,
            Description = recipe.Description,
            Servings = newServings,
            EstimatedCost = recipe.EstimatedCost * newServings / Math.Max(recipe.Servings, 1),
            CategoryTags = recipe.CategoryTags,
            IsCustom = recipe.IsCustom,
            ParentRecipeId = recipe.ParentRecipeId,
            CreatedAt = recipe.CreatedAt,
            CookingTime = recipe.CookingTime,
            Calories = recipe.Calories,
            ImageUrl = recipe.ImageUrl
        };

        var ratio = recipe.Servings > 0 ? (double)newServings / recipe.Servings : 1;

        scaledRecipe.Ingredients = recipe.Ingredients.Select(i => new Ingredient
        {
            Id = i.Id,
            Name = i.Name,
            Grams = Math.Round(i.Grams * ratio, 1),
            Category = i.Category,
            Tags = i.Tags,
            IconUrl = i.IconUrl
        }).ToList();

        scaledRecipe.Steps = recipe.Steps.Select(s => new CookingStep
        {
            StepNumber = s.StepNumber,
            Description = s.Description,
            DurationMinutes = s.DurationMinutes,
            HeatLevel = s.HeatLevel,
            Ingredients = s.Ingredients.Select(i => new Ingredient
            {
                Id = i.Id,
                Name = i.Name,
                Grams = Math.Round(i.Grams * ratio, 1),
                Category = i.Category,
                Tags = i.Tags,
                IconUrl = i.IconUrl
            }).ToList()
        }).ToList();

        return scaledRecipe;
    }

    public async Task SaveUserNoteAsync(UserNote note)
    {
        _userNotes.Add(note);
        UpdateFlavorProfile(note);
        await SaveNotesToFileAsync();
        await SaveFlavorProfileToFileAsync();
    }

    public Task<List<UserNote>> GetUserNotesAsync(string? recipeId = null)
    {
        var notes = recipeId == null
            ? _userNotes.ToList()
            : _userNotes.Where(n => n.RecipeId == recipeId).ToList();
        return Task.FromResult(notes);
    }

    public async Task DeleteUserNoteAsync(string noteId)
    {
        var note = _userNotes.FirstOrDefault(n => n.Id == noteId);
        if (note != null)
        {
            _userNotes.Remove(note);
            await SaveNotesToFileAsync();
        }
    }

    public Task<FlavorProfile> GetFlavorProfileAsync()
    {
        return Task.FromResult(_flavorProfile);
    }

    private void UpdateFlavorProfile(UserNote note)
    {
        if (!string.IsNullOrEmpty(note.Rating))
        {
            if (_flavorProfile.TastePreferences.ContainsKey(note.Rating))
                _flavorProfile.TastePreferences[note.Rating]++;
            else
                _flavorProfile.TastePreferences[note.Rating] = 1;
        }
    }

    // 笔记持久化方法
    private async Task LoadNotesFromFileAsync()
    {
        try
        {
            if (File.Exists(_notesFilePath))
            {
                var json = await File.ReadAllTextAsync(_notesFilePath);
                var notes = JsonSerializer.Deserialize<List<UserNote>>(json);
                if (notes != null)
                {
                    _userNotes.Clear();
                    _userNotes.AddRange(notes);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load notes: {ex.Message}");
        }
    }

    private async Task SaveNotesToFileAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_userNotes, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_notesFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save notes: {ex.Message}");
        }
    }

    private async Task LoadFlavorProfileFromFileAsync()
    {
        try
        {
            if (File.Exists(_flavorProfileFilePath))
            {
                var json = await File.ReadAllTextAsync(_flavorProfileFilePath);
                var profile = JsonSerializer.Deserialize<FlavorProfile>(json);
                if (profile != null)
                {
                    _flavorProfile.TastePreferences.Clear();
                    foreach (var kvp in profile.TastePreferences)
                    {
                        _flavorProfile.TastePreferences[kvp.Key] = kvp.Value;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load flavor profile: {ex.Message}");
        }
    }

    private async Task SaveFlavorProfileToFileAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_flavorProfile, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_flavorProfileFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save flavor profile: {ex.Message}");
        }
    }

    public Task SaveCustomRecipeAsync(Recipe recipe)
    {
        recipe.IsCustom = true;
        recipe.Id = Guid.NewGuid().ToString();
        recipe.CreatedAt = DateTime.Now;
        _recipes.Add(recipe);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 根据输入的食材名称智能匹配并"生成"推荐菜品
    /// </summary>
    public async Task<List<Recipe>> GenerateRecipesByIngredientAsync(string ingredientInput)
    {
        await EnsureInitializedAsync();

        if (string.IsNullOrWhiteSpace(ingredientInput))
            return new List<Recipe>();

        // 解析输入的食材关键词
        var keywords = ingredientInput
            .Split(new[] { ',', '，', ' ', '、' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim().ToLower())
            .Where(k => k.Length > 0)
            .ToList();

        if (!keywords.Any())
            return new List<Recipe>();

        // 尝试从 API 搜索
        try
        {
            // 用第一个关键词搜索
            var searchKeyword = keywords.First();
            
            _isLoading = true;
            _dataSource = "loading";
            OnPropertyChanged?.Invoke(nameof(IsLoading));
            OnPropertyChanged?.Invoke(nameof(DataSource));
            
            var response = await _httpClient.GetFromJsonAsync<MealDbResponse>($"{_apiBaseUrl}/filter.php?i={Uri.EscapeDataString(searchKeyword)}");
            
            if (response?.Meals != null && response.Meals.Any())
            {
                // 获取详情
                var detailTasks = response.Meals.Take(5).Select(async m =>
                {
                    try
                    {
                        var detail = await _httpClient.GetFromJsonAsync<MealDbResponse>($"{_apiBaseUrl}/lookup.php?i={m.IdMeal}");
                        return detail?.Meals?.FirstOrDefault();
                    }
                    catch { return null; }
                });
                
                var details = await Task.WhenAll(detailTasks);
                var recipes = details
                    .Where(m => m != null)
                    .Select(MapMealToRecipe)
                    .ToList();
                
                _dataSource = "api";
                OnPropertyChanged?.Invoke(nameof(DataSource));
                
                return recipes;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API ingredient search failed: {ex.Message}");
            _lastError = ex.Message;
        }
        finally
        {
            _isLoading = false;
            OnPropertyChanged?.Invoke(nameof(IsLoading));
        }

        // 回退到本地匹配
        _dataSource = "local";
        OnPropertyChanged?.Invoke(nameof(DataSource));
        
        var scored = _recipes.Select(recipe =>
        {
            var recipeText = (recipe.Name + recipe.Description +
                string.Join(" ", recipe.Ingredients.Select(i => i.Name)) +
                string.Join(" ", recipe.CategoryTags)).ToLower();

            int score = 0;
            foreach (var keyword in keywords)
            {
                if (recipe.Ingredients.Any(i => i.Name.ToLower().Contains(keyword) || keyword.Contains(i.Name.ToLower())))
                    score += 15;
                else if (recipe.Name.ToLower().Contains(keyword))
                    score += 10;
                else if (recipeText.Contains(keyword))
                    score += 5;
            }

            return (Recipe: recipe, Score: score);
        })
        .Where(x => x.Score > 0)
        .OrderByDescending(x => x.Score)
        .Take(5)
        .Select(x => x.Recipe)
        .ToList();

        return scored;
    }

    public Task<List<Recipe>> GetRecommendedRecipesAsync()
    {
        var recommended = _recipes.AsEnumerable();

        if (_flavorProfile.PreferredCategories.Any())
        {
            recommended = recommended.Where(r =>
                r.CategoryTags.Any(c => _flavorProfile.PreferredCategories.Contains(c)));
        }

        return Task.FromResult(recommended.Take(5).ToList());
    }
}

// TheMealDB API 响应模型
internal class MealDbResponse
{
    [JsonPropertyName("meals")]
    public List<MealDbMeal>? Meals { get; set; }
}

internal class MealDbMeal
{
    [JsonPropertyName("idMeal")]
    public string? IdMeal { get; set; }
    
    [JsonPropertyName("strMeal")]
    public string? StrMeal { get; set; }
    
    [JsonPropertyName("strDrinkAlternate")]
    public string? StrDrinkAlternate { get; set; }
    
    [JsonPropertyName("strCategory")]
    public string? StrCategory { get; set; }
    
    [JsonPropertyName("strArea")]
    public string? StrArea { get; set; }
    
    [JsonPropertyName("strInstructions")]
    public string? StrInstructions { get; set; }
    
    [JsonPropertyName("strMealThumb")]
    public string? StrMealThumb { get; set; }
    
    [JsonPropertyName("strTags")]
    public string? StrTags { get; set; }
    
    [JsonPropertyName("strYoutube")]
    public string? StrYoutube { get; set; }
    
    // 动态属性 strIngredient1-20 和 strMeasure1-20
    [JsonPropertyName("strIngredient1")]
    public string? StrIngredient1 { get; set; }
    [JsonPropertyName("strIngredient2")]
    public string? StrIngredient2 { get; set; }
    [JsonPropertyName("strIngredient3")]
    public string? StrIngredient3 { get; set; }
    [JsonPropertyName("strIngredient4")]
    public string? StrIngredient4 { get; set; }
    [JsonPropertyName("strIngredient5")]
    public string? StrIngredient5 { get; set; }
    [JsonPropertyName("strIngredient6")]
    public string? StrIngredient6 { get; set; }
    [JsonPropertyName("strIngredient7")]
    public string? StrIngredient7 { get; set; }
    [JsonPropertyName("strIngredient8")]
    public string? StrIngredient8 { get; set; }
    [JsonPropertyName("strIngredient9")]
    public string? StrIngredient9 { get; set; }
    [JsonPropertyName("strIngredient10")]
    public string? StrIngredient10 { get; set; }
    [JsonPropertyName("strIngredient11")]
    public string? StrIngredient11 { get; set; }
    [JsonPropertyName("strIngredient12")]
    public string? StrIngredient12 { get; set; }
    [JsonPropertyName("strIngredient13")]
    public string? StrIngredient13 { get; set; }
    [JsonPropertyName("strIngredient14")]
    public string? StrIngredient14 { get; set; }
    [JsonPropertyName("strIngredient15")]
    public string? StrIngredient15 { get; set; }
    [JsonPropertyName("strIngredient16")]
    public string? StrIngredient16 { get; set; }
    [JsonPropertyName("strIngredient17")]
    public string? StrIngredient17 { get; set; }
    [JsonPropertyName("strIngredient18")]
    public string? StrIngredient18 { get; set; }
    [JsonPropertyName("strIngredient19")]
    public string? StrIngredient19 { get; set; }
    [JsonPropertyName("strIngredient20")]
    public string? StrIngredient20 { get; set; }
    
    [JsonPropertyName("strMeasure1")]
    public string? StrMeasure1 { get; set; }
    [JsonPropertyName("strMeasure2")]
    public string? StrMeasure2 { get; set; }
    [JsonPropertyName("strMeasure3")]
    public string? StrMeasure3 { get; set; }
    [JsonPropertyName("strMeasure4")]
    public string? StrMeasure4 { get; set; }
    [JsonPropertyName("strMeasure5")]
    public string? StrMeasure5 { get; set; }
    [JsonPropertyName("strMeasure6")]
    public string? StrMeasure6 { get; set; }
    [JsonPropertyName("strMeasure7")]
    public string? StrMeasure7 { get; set; }
    [JsonPropertyName("strMeasure8")]
    public string? StrMeasure8 { get; set; }
    [JsonPropertyName("strMeasure9")]
    public string? StrMeasure9 { get; set; }
    [JsonPropertyName("strMeasure10")]
    public string? StrMeasure10 { get; set; }
    [JsonPropertyName("strMeasure11")]
    public string? StrMeasure11 { get; set; }
    [JsonPropertyName("strMeasure12")]
    public string? StrMeasure12 { get; set; }
    [JsonPropertyName("strMeasure13")]
    public string? StrMeasure13 { get; set; }
    [JsonPropertyName("strMeasure14")]
    public string? StrMeasure14 { get; set; }
    [JsonPropertyName("strMeasure15")]
    public string? StrMeasure15 { get; set; }
    [JsonPropertyName("strMeasure16")]
    public string? StrMeasure16 { get; set; }
    [JsonPropertyName("strMeasure17")]
    public string? StrMeasure17 { get; set; }
    [JsonPropertyName("strMeasure18")]
    public string? StrMeasure18 { get; set; }
    [JsonPropertyName("strMeasure19")]
    public string? StrMeasure19 { get; set; }
    [JsonPropertyName("strMeasure20")]
    public string? StrMeasure20 { get; set; }
}