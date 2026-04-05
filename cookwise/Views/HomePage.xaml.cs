using Microsoft.Maui.ApplicationModel;

namespace cookwise.Views;

public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
	}
	
	/// <summary>
	/// 点击 API 链接时打开 TheMealDB 网站
	/// </summary>
	private async void OnApiLinkTapped(object sender, EventArgs e)
	{
		try
		{
			var uri = new Uri("https://www.themealdb.com/");
			await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Failed to open browser: {ex.Message}");
		}
	}
}