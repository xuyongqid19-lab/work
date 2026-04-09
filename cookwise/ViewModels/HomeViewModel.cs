using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using cookwise.Models;
using cookwise.Services;namespace cookwise.ViewModels;
    public partial class HomeViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Recipe> _featuredRecipes = new();

        [ObservableProperty]
        private ObservableCollection<Recipe> _allRecipes = new();

        [ObservableProperty]
        private Recipe? _selectedRecipe;

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
        }        private async void LoadRecipes()
        {var service = RecipeService.Instance;
            
            try
            {
                // Only load local recipes
                var recipes = await service.GetAllRecipesAsync();
                AllRecipes = new ObservableCollection<Recipe>(recipes);
                
                // Set featured recipes (random selection from local data)
                var random = new Random();
                var featured = recipes.OrderBy(x => random.Next()).Take(3).ToList();
                FeaturedRecipes = new ObservableCollection<Recipe>(featured);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load recipes: {ex.Message}");
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