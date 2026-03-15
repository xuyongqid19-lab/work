using Microsoft.Maui;
using Microsoft.Maui.Controls;
using cookwise.Views;

namespace cookwise;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new NavigationPage(new LoginPage());
    }
}
