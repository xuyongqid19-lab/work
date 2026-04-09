using Microsoft.Maui.Controls;

namespace cookwise.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // Show loading overlay
        LoadingOverlay.IsVisible = true;
        
        // Wait 2-3 seconds
        await Task.Delay(2500);
        
        // Navigate to home page
        Application.Current.MainPage = new AppShell();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // Show loading overlay
        LoadingOverlay.IsVisible = true;
        
        // Wait 2-3 seconds
        await Task.Delay(2500);
        
        // Navigate to home page
        Application.Current.MainPage = new AppShell();
    }

    private async void OnContinueAsGuestClicked(object sender, EventArgs e)
    {
        // Show loading overlay
        LoadingOverlay.IsVisible = true;
        
        // Wait 2-3 seconds
        await Task.Delay(2500);
        
        // Navigate to home page
        Application.Current.MainPage = new AppShell();
    }

    private void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        DisplayAlert("Forgot Password", "Password reset instructions will be sent to your email.", "OK");
    }
}