using System;
using System.Collections.Generic;
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
        private readonly string _baseUrl = "https://cook.yunyoujun.cn/api";
        
        private List<Recipe> _recipes = new();
        private readonly List<UserNote> _userNotes = new();
        private readonly FlavorProfile _flavorProfile = new();
        private bool _isInitialized = false;

        private RecipeService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        private async Task EnsureInitializedAsync()
        {
            if (_isInitialized) return;
            
            try
            {
                var apiRecipes = await _httpClient.GetFromJsonAsync<List<ApiRecipe>>("/recipes");
                
                if (apiRecipes != null)
                {
                    _recipes = apiRecipes.Select(MapToRecipe).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to fetch from API: {ex.Message}");
                // Fallback to sample data if API fails
                InitializeSampleData();
            }
            
            _isInitialized = true;
        }

        private Recipe MapToRecipe(ApiRecipe api)
        {
            var recipe = new Recipe
            {
                Id = api.Id.ToString(),
                Name = api.Name,
                Description = api.Description ?? "",
                Servings = api.Servings,
                EstimatedCost = api.EstimatedCost ?? 0,
                CategoryTags = api.Categories ?? new List<string>()
            };

            // Map ingredients
            if (api.Ingredients != null)
            {
                recipe.Ingredients = api.Ingredients.Select(i => new Ingredient
                {
                    Name = i.Name,
                    Grams = i.Amount ?? 0,
                    Category = i.Unit ?? ""
                }).ToList();
            }

            // Map steps
            if (api.Steps != null)
            {
                recipe.Steps = api.Steps.Select((s, index) => new CookingStep
                {
                    StepNumber = index + 1,
                    Description = s,
                    DurationMinutes = null,
                    HeatLevel = null
                }).ToList();
            }

            return recipe;
        }

        private void InitializeSampleData()
        {
            // 番茄炒蛋
            var tomatoEgg = new Recipe
            {
                Id = "recipe_001",
                Name = "番茄炒蛋",
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
            };
            _recipes.Add(tomatoEgg);

            // 红烧肉
            var braisedPork = new Recipe
            {
                Id = "recipe_002",
                Name = "红烧肉",
                Description = "肥而不腻，入口即化，下饭神器",
                Servings = 4,
                EstimatedCost = 25.00,
                CategoryTags = new List<string> { "家常菜", "硬菜", "下饭菜" },
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "五花肉", Grams = 500, Category = "肉类" },
                    new() { Name = "生抽", Grams = 30, Category = "调料" },
                    new() { Name = "老抽", Grams = 15, Category = "调料" },
                    new() { Name = "白糖", Grams = 30, Category = "调料" },
                    new() { Name = "料酒", Grams = 20, Category = "调料" },
                    new() { Name = "八角", Grams = 2, Category = "调料" },
                    new() { Name = "桂皮", Grams = 3, Category = "调料" },
                    new() { Name = "姜片", Grams = 10, Category = "调料" }
                },
                Steps = new List<CookingStep>
                {
                    new() { StepNumber = 1, Description = "五花肉切块，冷水下锅加料酒焯水", DurationMinutes = 10, HeatLevel = "大火" },
                    new() { StepNumber = 2, Description = "锅中放油，加入白糖炒至焦糖色", DurationMinutes = 3, HeatLevel = "中小火" },
                    new() { StepNumber = 3, Description = "放入五花肉翻炒上色", DurationMinutes = 2, HeatLevel = "中火" },
                    new() { StepNumber = 4, Description = "加入生抽、老抽、料酒和香料", DurationMinutes = 1 },
                    new() { StepNumber = 5, Description = "加开水没过肉块，小火炖1小时", DurationMinutes = 60, HeatLevel = "小火" },
                    new() { StepNumber = 6, Description = "大火收汁至浓稠即可", DurationMinutes = 10, HeatLevel = "大火" }
                }
            };
            _recipes.Add(braisedPork);

            // 宫保鸡丁
            var kungPaoChicken = new Recipe
            {
                Id = "recipe_003",
                Name = "宫保鸡丁",
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
                    new() { Name = "葱段", Grams = 20, Category = "蔬菜" },
                    new() { Name = "蒜末", Grams = 5, Category = "调料" },
                    new() { Name = "生抽", Grams = 15, Category = "调料" },
                    new() { Name = "醋", Grams = 10, Category = "调料" },
                    new() { Name = "糖", Grams = 10, Category = "调料" },
                    new() { Name = "淀粉", Grams = 10, Category = "调料" }
                },
                Steps = new List<CookingStep>
                {
                    new() { StepNumber = 1, Description = "鸡胸肉切丁，用盐、淀粉腌制10分钟", DurationMinutes = 10 },
                    new() { StepNumber = 2, Description = "调好酱汁：生抽+醋+糖+水淀粉", DurationMinutes = 1 },
                    new() { StepNumber = 3, Description = "热锅凉油下花生米炒至金黄酥脆", DurationMinutes = 2, HeatLevel = "小火" },
                    new() { StepNumber = 4, Description = "下鸡丁炒至变色盛出备用", DurationMinutes = 2, HeatLevel = "中火" },
                    new() { StepNumber = 5, Description = "炒香干辣椒和花椒", DurationMinutes = 1, HeatLevel = "小火" },
                    new() { StepNumber = 6, Description = "放入鸡丁和酱汁快速翻炒", DurationMinutes = 1, HeatLevel = "大火" },
                    new() { StepNumber = 7, Description = "加入花生米和葱段翻匀出锅", DurationMinutes = 1 }
                }
            };
            _recipes.Add(kungPaoChicken);

            // 麻婆豆腐
            var mapoTofu = new Recipe
            {
                Id = "recipe_004",
                Name = "麻婆豆腐",
                Description = "麻辣鲜香，豆腐嫩滑，川菜经典",
                Servings = 2,
                EstimatedCost = 8.00,
                CategoryTags = new List<string> { "川菜", "辣味", "素菜" },
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "嫩豆腐", Grams = 300, Category = "豆制品" },
                    new() { Name = "猪肉末", Grams = 100, Category = "肉类" },
                    new() { Name = "郫县豆瓣酱", Grams = 30, Category = "调料" },
                    new() { Name = "花椒粉", Grams = 3, Category = "调料" },
                    new() { Name = "蒜末", Grams = 5, Category = "调料" },
                    new() { Name = "姜末", Grams = 3, Category = "调料" },
                    new() { Name = "生抽", Grams = 10, Category = "调料" },
                    new() { Name = "淀粉", Grams = 10, Category = "调料" },
                    new() { Name = "食用油", Grams = 20, Category = "调料" }
                },
                Steps = new List<CookingStep>
                {
                    new() { StepNumber = 1, Description = "豆腐切小块，用盐水焯水去腥", DurationMinutes = 2, HeatLevel = "大火" },
                    new() { StepNumber = 2, Description = "热锅下油，炒香姜蒜末和豆瓣酱", DurationMinutes = 1, HeatLevel = "中小火" },
                    new() { StepNumber = 3, Description = "放入肉末炒散至变色", DurationMinutes = 2, HeatLevel = "中火" },
                    new() { StepNumber = 4, Description = "加适量水，放入豆腐块", DurationMinutes = 1 },
                    new() { StepNumber = 5, Description = "加入生抽，中火煮3分钟", DurationMinutes = 3, HeatLevel = "中火" },
                    new() { StepNumber = 6, Description = "用水淀粉勾芡，撒上花椒粉", DurationMinutes = 1 }
                }
            };
            _recipes.Add(mapoTofu);

            // 糖醋排骨
            var sweetSourRibs = new Recipe
            {
                Id = "recipe_005",
                Name = "糖醋排骨",
                Description = "酸甜可口，外酥里嫩，老少皆宜",
                Servings = 3,
                EstimatedCost = 22.00,
                CategoryTags = new List<string> { "家常菜", "酸甜", "硬菜" },
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "排骨", Grams = 400, Category = "肉类" },
                    new() { Name = "白糖", Grams = 40, Category = "调料" },
                    new() { Name = "醋", Grams = 30, Category = "调料" },
                    new() { Name = "生抽", Grams = 20, Category = "调料" },
                    new() { Name = "老抽", Grams = 10, Category = "调料" },
                    new() { Name = "料酒", Grams = 15, Category = "调料" },
                    new() { Name = "姜片", Grams = 10, Category = "调料" },
                    new() { Name = "淀粉", Grams = 20, Category = "调料" },
                    new() { Name = "食用油", Grams = 30, Category = "调料" }
                },
                Steps = new List<CookingStep>
                {
                    new() { StepNumber = 1, Description = "排骨切段，焯水去血沫", DurationMinutes = 8, HeatLevel = "大火" },
                    new() { StepNumber = 2, Description = "排骨裹淀粉，油炸至金黄", DurationMinutes = 8, HeatLevel = "中火" },
                    new() { StepNumber = 3, Description = "调糖醋汁：糖+醋+生抽+老抽+水", DurationMinutes = 1 },
                    new() { StepNumber = 4, Description = "锅中留底油，放入排骨和糖醋汁", DurationMinutes = 1 },
                    new() { StepNumber = 5, Description = "中火翻炒至汁液浓稠包裹排骨", DurationMinutes = 3, HeatLevel = "中火" }
                }
            };
            _recipes.Add(sweetSourRibs);

            // 西红柿鸡蛋汤
            var tomatoEggSoup = new Recipe
            {
                Id = "recipe_006",
                Name = "西红柿鸡蛋汤",
                Description = "清淡鲜美，简单快手，营养丰富",
                Servings = 2,
                EstimatedCost = 3.00,
                CategoryTags = new List<string> { "快手菜", "汤类", "家常菜" },
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "番茄", Grams = 150, Category = "蔬菜" },
                    new() { Name = "鸡蛋", Grams = 60, Category = "蛋白质" },
                    new() { Name = "盐", Grams = 2, Category = "调料" },
                    new() { Name = "香油", Grams = 5, Category = "调料" },
                    new() { Name = "葱花", Grams = 5, Category = "调料" }
                },
                Steps = new List<CookingStep>
                {
                    new() { StepNumber = 1, Description = "番茄切块，鸡蛋打散", DurationMinutes = 2 },
                    new() { StepNumber = 2, Description = "锅中加水烧开，放入番茄块", DurationMinutes = 3, HeatLevel = "大火" },
                    new() { StepNumber = 3, Description = "水开后转中火，慢慢倒入蛋液", DurationMinutes = 1, HeatLevel = "中火" },
                    new() { StepNumber = 4, Description = "关火，加盐调味，淋香油撒葱花", DurationMinutes = 1 }
                }
            };
            _recipes.Add(tomatoEggSoup);

            // 可乐鸡翅
            var colaChickenWings = new Recipe
            {
                Id = "recipe_007",
                Name = "可乐鸡翅",
                Description = "香甜软嫩，老少皆爱的懒人菜",
                Servings = 2,
                EstimatedCost = 12.00,
                CategoryTags = new List<string> { "懒人菜", "家常菜", "肉类" },
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "鸡翅中", Grams = 300, Category = "肉类" },
                    new() { Name = "可乐", Grams = 330, Category = "饮料" },
                    new() { Name = "生抽", Grams = 20, Category = "调料" },
                    new() { Name = "老抽", Grams = 10, Category = "调料" },
                    new() { Name = "姜片", Grams = 10, Category = "调料" },
                    new() { Name = "料酒", Grams = 10, Category = "调料" }
                },
                Steps = new List<CookingStep>
                {
                    new() { StepNumber = 1, Description = "鸡翅洗净，表面划两刀", DurationMinutes = 2 },
                    new() { StepNumber = 2, Description = "冷水下锅，加料酒姜片焯水", DurationMinutes = 5, HeatLevel = "大火" },
                    new() { StepNumber = 3, Description = "热锅下油，放入鸡翅煎至两面金黄", DurationMinutes = 5, HeatLevel = "中火" },
                    new() { StepNumber = 4, Description = "倒入可乐（没过鸡翅）、生抽、老抽", DurationMinutes = 1 },
                    new() { StepNumber = 5, Description = "大火烧开后转中小火炖15分钟", DurationMinutes = 15, HeatLevel = "中小火" },
                    new() { StepNumber = 6, Description = "大火收汁至浓稠即可", DurationMinutes = 5, HeatLevel = "大火" }
                }
            };
            _recipes.Add(colaChickenWings);

            // 蒜蓉西兰花
            var garlicBroccoli = new Recipe
            {
                Id = "recipe_008",
                Name = "蒜蓉西兰花",
                Description = "清脆爽口，蒜香浓郁，健康素菜",
                Servings = 2,
                EstimatedCost = 5.00,
                CategoryTags = new List<string> { "素菜", "快手菜", "健康" },
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "西兰花", Grams = 300, Category = "蔬菜" },
                    new() { Name = "大蒜", Grams = 15, Category = "调料" },
                    new() { Name = "盐", Grams = 2, Category = "调料" },
                    new() { Name = "食用油", Grams = 15, Category = "调料" },
                    new() { Name = "水淀粉", Grams = 10, Category = "调料" }
                },
                Steps = new List<CookingStep>
                {
                    new() { StepNumber = 1, Description = "西兰花切小朵，用盐水浸泡洗净", DurationMinutes = 3 },
                    new() { StepNumber = 2, Description = "大蒜切末备用", DurationMinutes = 1 },
                    new() { StepNumber = 3, Description = "锅中烧水，加盐和油，焯西兰花2分钟", DurationMinutes = 2, HeatLevel = "大火" },
                    new() { StepNumber = 4, Description = "热锅下油，爆香蒜末", DurationMinutes = 1, HeatLevel = "小火" },
                    new() { StepNumber = 5, Description = "放入西兰花快速翻炒，加盐调味", DurationMinutes = 1 },
                    new() { StepNumber = 6, Description = "用水淀粉勾芡出锅", DurationMinutes = 1 }
                }
            };
            _recipes.Add(garlicBroccoli);

            // 青椒肉丝
            var greenPepperPork = new Recipe
            {
                Id = "recipe_009",
                Name = "青椒肉丝",
                Description = "清香爽口，色彩诱人，家常快手菜",
                Servings = 2,
                EstimatedCost = 10.00,
                CategoryTags = new List<string> { "家常菜", "快手菜", "下饭菜" },
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "猪里脊肉", Grams = 150, Category = "肉类" },
                    new() { Name = "青椒", Grams = 100, Category = "蔬菜" },
                    new() { Name = "红椒", Grams = 50, Category = "蔬菜" },
                    new() { Name = "生抽", Grams = 15, Category = "调料" },
                    new() { Name = "淀粉", Grams = 10, Category = "调料" },
                    new() { Name = "盐", Grams = 2, Category = "调料" },
                    new() { Name = "食用油", Grams = 15, Category = "调料" }
                },
                Steps = new List<CookingStep>
                {
                    new() { StepNumber = 1, Description = "里脊肉切丝，用淀粉和少量生抽腌制", DurationMinutes = 5 },
                    new() { StepNumber = 2, Description = "青红椒切丝备用", DurationMinutes = 2 },
                    new() { StepNumber = 3, Description = "热锅下油，下肉丝炒至变色", DurationMinutes = 2, HeatLevel = "中火" },
                    new() { StepNumber = 4, Description = "放入青红椒丝翻炒1分钟", DurationMinutes = 1, HeatLevel = "大火" },
                    new() { StepNumber = 5, Description = "加盐和生抽调味，翻炒均匀出锅", DurationMinutes = 1 }
                }
            };
            _recipes.Add(greenPepperPork);

            // 酸辣土豆丝
            var spicySourPotato = new Recipe
            {
                Id = "recipe_010",
                Name = "酸辣土豆丝",
                Description = "爽脆可口，酸辣开胃，简单实惠",
                Servings = 2,
                EstimatedCost = 2.50,
                CategoryTags = new List<string> { "快手菜", "家常菜", "酸辣" },
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "土豆", Grams = 200, Category = "蔬菜" },
                    new() { Name = "干辣椒", Grams = 5, Category = "调料" },
                    new() { Name = "花椒", Grams = 2, Category = "调料" },
                    new() { Name = "醋", Grams = 20, Category = "调料" },
                    new() { Name = "盐", Grams = 2, Category = "调料" },
                    new() { Name = "食用油", Grams = 15, Category = "调料" },
                    new() { Name = "葱段", Grams = 10, Category = "调料" }
                },
                Steps = new List<CookingStep>
                {
                    new() { StepNumber = 1, Description = "土豆切细丝，用清水冲洗去淀粉", DurationMinutes = 2 },
                    new() { StepNumber = 2, Description = "热锅下油，放入干辣椒和花椒爆香", DurationMinutes = 1, HeatLevel = "小火" },
                    new() { StepNumber = 3, Description = "放入土豆丝大火快速翻炒", DurationMinutes = 1, HeatLevel = "大火" },
                    new() { StepNumber = 4, Description = "沿锅边淋入醋，继续翻炒", DurationMinutes = 1 },
                    new() { StepNumber = 5, Description = "加盐调味，放入葱段翻匀出锅", DurationMinutes = 1 }
                }
            };
            _recipes.Add(spicySourPotato);

            // 红烧茄子
            var braisedEggplant = new Recipe
            {
                Id = "recipe_011",
                Name = "红烧茄子",
                Description = "软糯入味，酱香浓郁，下饭神器",
                Servings = 2,
                EstimatedCost = 4.00,
                CategoryTags = new List<string> { "家常菜", "素菜", "下饭菜" },
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "茄子", Grams = 300, Category = "蔬菜" },
                    new() { Name = "大蒜", Grams = 10, Category = "调料" },
                    new() { Name = "生抽", Grams = 20, Category = "调料" },
                    new() { Name = "老抽", Grams = 5, Category = "调料" },
                    new() { Name = "糖", Grams = 5, Category = "调料" },
                    new() { Name = "盐", Grams = 2, Category = "调料" },
                    new() { Name = "食用油", Grams = 25, Category = "调料" }
                },
                Steps = new List<CookingStep>
                {
                    new() { StepNumber = 1, Description = "茄子切滚刀块，用盐腌制10分钟", DurationMinutes = 10 },
                    new() { StepNumber = 2, Description = "热锅下油，放入茄子煎软", DurationMinutes = 4, HeatLevel = "中火" },
                    new() { StepNumber = 3, Description = "放入蒜末爆香", DurationMinutes = 1, HeatLevel = "小火" },
                    new() { StepNumber = 4, Description = "加入生抽、老抽、糖和适量水", DurationMinutes = 1 },
                    new() { StepNumber = 5, Description = "盖锅焖煮3分钟，大火收汁", DurationMinutes = 4, HeatLevel = "中火" }
                }
            };
            _recipes.Add(braisedEggplant);

            // 鱼香肉丝
            var yuxiangPork = new Recipe
            {
                Id = "recipe_012",
                Name = "鱼香肉丝",
                Description = "咸甜酸辣兼备，木耳脆爽，川菜经典",
                Servings = 2,
                EstimatedCost = 12.00,
                CategoryTags = new List<string> { "川菜", "下饭菜", "家常菜" },
                Ingredients = new List<Ingredient>
                {
                    new() { Name = "猪里脊肉", Grams = 150, Category = "肉类" },
                    new() { Name = "木耳", Grams = 30, Category = "蔬菜" },
                    new() { Name = "胡萝卜", Grams = 50, Category = "蔬菜" },
                    new() { Name = "郫县豆瓣酱", Grams = 15, Category = "调料" },
                    new() { Name = "葱姜蒜", Grams = 10, Category = "调料" },
                    new() { Name = "生抽", Grams = 10, Category = "调料" },
                    new() { Name = "醋", Grams = 10, Category = "调料" },
                    new() { Name = "糖", Grams = 10, Category = "调料" },
                    new() { Name = "淀粉", Grams = 10, Category = "调料" }
                },
                Steps = new List<CookingStep>
                {
                    new() { StepNumber = 1, Description = "里脊肉切丝，用淀粉腌制", DurationMinutes = 5 },
                    new() { StepNumber = 2, Description = "木耳、胡萝卜切丝，葱姜蒜切末", DurationMinutes = 3 },
                    new() { StepNumber = 3, Description = "调鱼香汁：生抽+醋+糖+水淀粉", DurationMinutes = 1 },
                    new() { StepNumber = 4, Description = "热锅下油，炒肉丝至变色盛出", DurationMinutes = 2, HeatLevel = "中火" },
                    new() { StepNumber = 5, Description = "炒香豆瓣酱和葱姜蒜", DurationMinutes = 1, HeatLevel = "小火" },
                    new() { StepNumber = 6, Description = "放入配菜翻炒，加入肉丝和鱼香汁", DurationMinutes = 2, HeatLevel = "大火" }
                }
            };
            _recipes.Add(yuxiangPork);
        }

        public async Task<List<Recipe>> GetAllRecipesAsync()
        {
            await EnsureInitializedAsync();
            return _recipes.ToList();
        }

        public async Task<Recipe?> GetRecipeByIdAsync(string id)
        {
            await EnsureInitializedAsync();
            var recipe = _recipes.FirstOrDefault(r => r.Id == id);
            
            // Try to fetch from API if not found locally
            if (recipe == null)
            {
                try
                {
                    var apiRecipe = await _httpClient.GetFromJsonAsync<ApiRecipe>($"/recipes/{id}");
                    if (apiRecipe != null)
                    {
                        recipe = MapToRecipe(apiRecipe);
                    }
                }
                catch { }
            }
            
            return recipe;
        }

        public async Task<List<Recipe>> SearchRecipesAsync(string keyword)
        {
            await EnsureInitializedAsync();
            
            if (string.IsNullOrWhiteSpace(keyword))
                return _recipes.ToList();

            // Try API search first
            try
            {
                var apiResults = await _httpClient.GetFromJsonAsync<List<ApiRecipe>>($"/recipes/search?q={keyword}");
                if (apiResults != null && apiResults.Any())
                {
                    return apiResults.Select(MapToRecipe).ToList();
                }
            }
            catch { }

            // Fallback to local search
            var lowerKeyword = keyword.ToLower();
            var results = _recipes.Where(r =>
                r.Name.ToLower().Contains(lowerKeyword) ||
                r.Description.ToLower().Contains(lowerKeyword) ||
                r.CategoryTags.Any(t => t.ToLower().Contains(lowerKeyword)) ||
                r.Ingredients.Any(i => i.Name.ToLower().Contains(lowerKeyword))
            ).ToList();

            return results;
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
                CreatedAt = recipe.CreatedAt
            };

            var ratio = recipe.Servings > 0 ? (double)newServings / recipe.Servings : 1;

            scaledRecipe.Ingredients = recipe.Ingredients.Select(i => new Ingredient
            {
                Id = i.Id,
                Name = i.Name,
                Grams = Math.Round(i.Grams * ratio, 1),
                Category = i.Category,
                Tags = i.Tags
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
                    Tags = i.Tags
                }).ToList()
            }).ToList();

            return scaledRecipe;
        }

        public Task SaveUserNoteAsync(UserNote note)
        {
            _userNotes.Add(note);
            UpdateFlavorProfile(note);
            return Task.CompletedTask;
        }

        public Task<List<UserNote>> GetUserNotesAsync(string? recipeId = null)
        {
            var notes = recipeId == null
                ? _userNotes.ToList()
                : _userNotes.Where(n => n.RecipeId == recipeId).ToList();
            return Task.FromResult(notes);
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

        public Task SaveCustomRecipeAsync(Recipe recipe)
        {
            recipe.IsCustom = true;
            recipe.Id = Guid.NewGuid().ToString();
            recipe.CreatedAt = DateTime.Now;
            _recipes.Add(recipe);
            return Task.CompletedTask;
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

        // API Response Models
        private class ApiRecipe
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            
            [JsonPropertyName("name")]
            public string Name { get; set; } = "";
            
            [JsonPropertyName("description")]
            public string? Description { get; set; }
            
            [JsonPropertyName("servings")]
            public int Servings { get; set; }
            
            [JsonPropertyName("estimatedCost")]
            public double? EstimatedCost { get; set; }
            
            [JsonPropertyName("categories")]
            public List<string>? Categories { get; set; }
            
            [JsonPropertyName("ingredients")]
            public List<ApiIngredient>? Ingredients { get; set; }
            
            [JsonPropertyName("steps")]
            public List<string>? Steps { get; set; }
        }

        private class ApiIngredient
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
        
        [JsonPropertyName("amount")]
        public double? Amount { get; set; }
        
        [JsonPropertyName("unit")]
        public string? Unit { get; set; }
    }
}
