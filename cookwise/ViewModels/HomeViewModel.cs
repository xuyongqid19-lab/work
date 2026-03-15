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

    public HomeViewModel()
    {
        LoadRecipes();
    }

    private async void LoadRecipes()
    {
        var service = RecipeService.Instance;
        var recipes = await service.GetAllRecipesAsync();

        AllRecipes = new ObservableCollection<Recipe>(recipes);
        
        // Featured = random 3 recipes for variety
        var random = new Random();
        var shuffled = recipes.OrderBy(x => random.Next()).ToList();
        var featured = shuffled.Take(3).ToList();
        FeaturedRecipes = new ObservableCollection<Recipe>(featured);
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
        await Shell.Current.DisplayAlert("口味画像", "基于您的口味记录，我们将为您推荐更合适的菜谱", "确定");
    }
}
