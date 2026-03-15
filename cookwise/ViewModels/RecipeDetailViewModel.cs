using System;
using System.Threading.Tasks;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using cookwise.Models;
using cookwise.Services;
using Timer = System.Timers.Timer;

namespace cookwise.ViewModels;
    public partial class RecipeDetailViewModel : ObservableObject
{
    [ObservableProperty]
    private Recipe _recipe = new();

    [ObservableProperty]
    private int _currentServings = 1;

    [ObservableProperty]
    private int _activeTimerSeconds = 0;

    [ObservableProperty]
    private string _activeTimerDisplay = "00:00";

    [ObservableProperty]
    private bool _hasActiveTimer = false;

    private Timer? _timer;

    public RecipeDetailViewModel()
    {
    }

    public async Task LoadRecipe(string recipeId)
    {
        var service = RecipeService.Instance;
        Recipe = await service.GetRecipeByIdAsync(recipeId) ?? new Recipe();
        CurrentServings = Recipe.Servings;
    }

    [RelayCommand]
    private void IncreaseServings()
    {
        CurrentServings++;
        var service = RecipeService.Instance;
        Recipe = service.ScaleRecipe(Recipe, CurrentServings);
        OnPropertyChanged(nameof(Recipe));
    }

    [RelayCommand]
    private void DecreaseServings()
    {
        if (CurrentServings > 1)
        {
            CurrentServings--;
            var service = RecipeService.Instance;
            Recipe = service.ScaleRecipe(Recipe, CurrentServings);
            OnPropertyChanged(nameof(Recipe));
        }
    }

    [RelayCommand]
    private void StartTimer(CookingStep step)
    {
        if (step.DurationMinutes.HasValue)
        {
            StopTimer();
            ActiveTimerSeconds = step.DurationMinutes.Value * 60;
            HasActiveTimer = true;
            UpdateTimerDisplay();

            _timer = new Timer(1000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        if (ActiveTimerSeconds > 0)
        {
            ActiveTimerSeconds--;
            UpdateTimerDisplay();
        }
        else
        {
            StopTimer();
        }
    }

    private void UpdateTimerDisplay()
    {
        var minutes = ActiveTimerSeconds / 60;
        var seconds = ActiveTimerSeconds % 60;
        ActiveTimerDisplay = $"{minutes:D2}:{seconds:D2}";
    }

    [RelayCommand]
    private void StopTimer()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
        }
        ActiveTimerSeconds = 0;
        HasActiveTimer = false;
        ActiveTimerDisplay = "00:00";
    }

    [RelayCommand]
    private async Task AddNote()
    {
        await Shell.Current.GoToAsync("//NotePage");
    }
}
