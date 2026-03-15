using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using cookwise.Models;
using cookwise.Services;

namespace cookwise.ViewModels;
    public partial class SearchViewModel : ObservableObject
{
    [ObservableProperty]
    private string _ingredientInput = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _selectedIngredients = new();

    [ObservableProperty]
    private ObservableCollection<SearchResult> _searchResults = new();

    [RelayCommand]
    private void AddIngredient()
    {
        if (!string.IsNullOrWhiteSpace(IngredientInput))
        {
            var ingredients = IngredientInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(i => i.Trim())
                .Where(i => !SelectedIngredients.Contains(i));
            
            foreach (var ing in ingredients)
            {
                SelectedIngredients.Add(ing);
            }
            IngredientInput = string.Empty;
        }
    }

    [RelayCommand]
    private void RemoveIngredient(string ingredient)
    {
        if (SelectedIngredients.Contains(ingredient))
        {
            SelectedIngredients.Remove(ingredient);
        }
    }

    [RelayCommand]
    private async Task Search()
    {
        if (!SelectedIngredients.Any())
            return;

        var service = RecipeService.Instance;
        var results = await service.SearchByIngredientsAsync(SelectedIngredients.ToList());
        SearchResults = new ObservableCollection<SearchResult>(results);
    }

    [RelayCommand]
    private async Task ViewRecipe(Recipe recipe)
    {
        if (recipe != null)
        {
            await Shell.Current.GoToAsync($"//RecipeDetailPage?recipeId={recipe.Id}");
        }
    }
}
