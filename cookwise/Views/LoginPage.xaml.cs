using Microsoft.Maui.Controls;

namespace cookwise.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private void OnLoginClicked(object sender, EventArgs e)
    {
        // Navigate to home page (or wherever appropriate after login)
        Application.Current.MainPage = new AppShell();
    }

    private void OnRegisterClicked(object sender, EventArgs e)
    {
        // For now, navigate to home page as well
        // In a real app, this might go to a registration page
        Application.Current.MainPage = new AppShell();
    }

    private void OnContinueAsGuestClicked(object sender, EventArgs e)
    {
        // Navigate to home page as guest
        Application.Current.MainPage = new AppShell();
    }

    private void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        // Handle forgot password logic
        // For now, just show a simple message or navigate to forgot password page
        DisplayAlert("Forgot Password", "Password reset instructions will be sent to your email.", "OK");
    }
}