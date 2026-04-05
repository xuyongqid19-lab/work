using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using cookwise.Models;
using cookwise.Services;

namespace cookwise.ViewModels;
    public partial class HomeViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Recipe> _featuredRecipes = new();

        [ObservableProperty]
        private ObservableCollection<Recipe> _allRecipes = new();

        [ObservableProperty]
        private Recipe? _selectedRecipe;
        
        [ObservableProperty]
        private bool _isLoading = true;
        
        [ObservableProperty]
        private string _dataSourceIndicator = "正在加载...";
        
        [ObservableProperty]
        private string _statusMessage = string.Empty;

        partial void OnSelectedRecipeChanged(Recipe? value)
        {
            if (value != null)
            {
                _ = GoToRecipeDetail(value);
                SelectedRecipe = null; // clear selection so it can be clicked again
            }
        }

        public HomeViewModel()
        {
            LoadRecipes();
        }

        private async void LoadRecipes()
        {
            IsLoading = true;
            DataSourceIndicator = "正在从 API 加载...";
            StatusMessage = string.Empty;
            
            var service = RecipeService.Instance;
            
            // 订阅状态变更
            service.OnPropertyChanged += (propertyName) =>
            {
                if (propertyName == nameof(RecipeService.IsLoading))
                {
                    // 状态已在服务中更新
                }
                else if (propertyName == nameof(RecipeService.DataSource))
                {
                    var source = service.DataSource;
                    DataSourceIndicator = source switch
                    {
                        "api" => "📡 在线数据",
                        "local" => "💾 离线数据",
                        "loading" => "⏳ 加载中...",
                        _ => "正在加载..."
                    };
                }
            };
            
            try
            {
                // 尝试加载 API 数据
                var apiRecipes = await service.GetRandomRecipesAsync(3);
                if (apiRecipes.Any())
                {
                    FeaturedRecipes = new ObservableCollection<Recipe>(apiRecipes);
                }
                
                // 获取全部菜谱
                var recipes = await service.GetAllRecipesAsync();
                AllRecipes = new ObservableCollection<Recipe>(recipes);
                
                // 更新状态
                DataSourceIndicator = service.DataSource == "api" ? "📡 在线数据" : "💾 离线数据";
                
                if (!string.IsNullOrEmpty(service.LastError))
                {
                    StatusMessage = $"使用本地数据（API 暂不可用）";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载失败: {ex.Message}";
                DataSourceIndicator = "⚠️ 加载失败";
            }
            finally
            {
                IsLoading = false;
            }
        }

    [RelayCommand]
    private async Task Search()
    {
        await Shell.Current.GoToAsync("//SearchPage");
    }

    [RelayCommand]
    private async Task Notes()
    {
        await Shell.Current.GoToAsync("//NotePage");
    }

    [RelayCommand]
    private async Task Profile()
    {
        await Shell.Current.DisplayAlert("Flavor Profile", "Based on your taste records, we will recommend more suitable recipes.", "OK");
    }

    [RelayCommand]
    private async Task GoToRecipeDetail(Recipe recipe)
    {
        if (recipe != null)
        {
            await Shell.Current.GoToAsync($"RecipeDetailPage?recipeId={recipe.Id}");
        }
    }
}
