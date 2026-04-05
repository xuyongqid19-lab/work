using System;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using cookwise.Models;
using cookwise.Services;
using Timer = System.Timers.Timer;

namespace cookwise.ViewModels;

[QueryProperty(nameof(RecipeId), "recipeId")]
public partial class RecipeDetailViewModel : ObservableObject
{
    private string _recipeId;
    public string RecipeId
    {
        get => _recipeId;
        set
        {
            _recipeId = value;
            _ = LoadRecipe(value);
        }
    }

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
    
    [ObservableProperty]
    private bool _isCountdownWarning = false;
    
    [ObservableProperty]
    private string _timerWarningText = string.Empty;

    private Timer? _timer;
    
    // 计时器震动配置
    private const int WarningThresholdSeconds = 10; // 最后10秒开始震动提醒
    private bool _hasVibratedForCurrentSecond = false;

    public RecipeDetailViewModel()
    {
    }

    public async Task LoadRecipe(string recipeId)
    {
        var service = RecipeService.Instance;
        var recipe = await service.GetRecipeByIdAsync(recipeId) ?? new Recipe();
        
        // 更新所有相关属性
        Recipe = recipe;
        CurrentServings = Recipe.Servings;
        
        // 显式通知绑定更新
        OnPropertyChanged(nameof(Recipe));
        OnPropertyChanged(nameof(Recipe.Steps));
        OnPropertyChanged(nameof(Recipe.Ingredients));
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
            IsCountdownWarning = false;
            TimerWarningText = string.Empty;
            _hasVibratedForCurrentSecond = false;
            UpdateTimerDisplay();

            _timer = new Timer(1000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }
    }

    private async void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        if (ActiveTimerSeconds > 0)
        {
            ActiveTimerSeconds--;
            UpdateTimerDisplay();
            
            // 最后10秒震动提醒
            if (ActiveTimerSeconds <= WarningThresholdSeconds && ActiveTimerSeconds > 0)
            {
                // 在主线程更新UI和触发震动
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    IsCountdownWarning = true;
                    TimerWarningText = $"⏱️ {ActiveTimerSeconds} 秒";
                    
                    // 每秒短震动
                    if (!_hasVibratedForCurrentSecond)
                    {
                        _hasVibratedForCurrentSecond = true;
                        try
                        {
                            // 短震动 (100ms)
                            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Vibration failed: {ex.Message}");
                        }
                    }
                });
                
                // 重置标志以便下一秒震动
                _hasVibratedForCurrentSecond = false;
            }
        }
        else
        {
            // 计时结束 - 长震动提醒
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                IsCountdownWarning = false;
                TimerWarningText = "✅ 时间到！";
                
                try
                {
                    // 长震动模式：震动3次，每次300ms，间隔200ms
                    for (int i = 0; i < 3; i++)
                    {
                        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
                        await Task.Delay(500);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Final vibration failed: {ex.Message}");
                }
            });
            
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
        IsCountdownWarning = false;
        TimerWarningText = string.Empty;
        ActiveTimerDisplay = "00:00";
    }

    [RelayCommand]
    private async Task AddNote()
    {
        await Shell.Current.GoToAsync("//NotePage");
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task SaveRecipe()
    {
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert("Success", "Recipe saved!", "OK");
        }
    }

    [RelayCommand]
    private void ToggleFavorite()
    {
        // Toggle favorite logic
    }
}
