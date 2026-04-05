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

    // AI 生成菜品相关属性
    [ObservableProperty]
    private string _aiIngredientInput = string.Empty;

    [ObservableProperty]
    private bool _isGenerating = false;

    [ObservableProperty]
    private ObservableCollection<Recipe> _generatedRecipes = new();

    [ObservableProperty]
    private bool _hasGeneratedResults = false;

    [ObservableProperty]
    private string _generateStatusText = string.Empty;

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
    private async Task GenerateRecipes()
    {
        if (string.IsNullOrWhiteSpace(AiIngredientInput))
            return;

        IsGenerating = true;
        HasGeneratedResults = false;
        GenerateStatusText = "AI 正在为您生成菜品...";
        GeneratedRecipes.Clear();

        try
        {
            var service = RecipeService.Instance;
            var recipes = await service.GenerateRecipesByIngredientAsync(AiIngredientInput.Trim());
            
            GeneratedRecipes = new ObservableCollection<Recipe>(recipes);
            HasGeneratedResults = GeneratedRecipes.Any();
            GenerateStatusText = HasGeneratedResults
                ? $"为「{AiIngredientInput}」找到 {GeneratedRecipes.Count} 道推荐菜品"
                : $"暂无与「{AiIngredientInput}」相关的菜品，试试其他食材吧";
        }
        catch (Exception ex)
        {
            GenerateStatusText = $"生成失败：{ex.Message}";
        }
        finally
        {
            IsGenerating = false;
        }
    }

    [RelayCommand]
    private async Task ViewRecipe(Recipe recipe)
    {
        if (recipe != null)
        {
            await Shell.Current.GoToAsync($"RecipeDetailPage?recipeId={recipe.Id}");
        }
    }
}
