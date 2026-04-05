using cookwise.Views;

namespace cookwise;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("RecipeDetailPage", typeof(RecipeDetailPage));
    }
}
