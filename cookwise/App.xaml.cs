using Microsoft.Maui;
using Microsoft.Maui.Controls;
using cookwise.Views;
using Plugin.LocalNotification;

namespace cookwise;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new NavigationPage(new LoginPage());
    }
    
    protected override async void OnStart()
    {
        base.OnStart();
        
        // Request notification permission
        if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
        {
            await LocalNotificationCenter.Current.RequestNotificationPermission();
        }
    }
}