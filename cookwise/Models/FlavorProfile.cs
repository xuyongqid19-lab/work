namespace cookwise.Models;

public class FlavorProfile
{
    public string UserId { get; set; } = "default";
    public Dictionary<string, int> TastePreferences { get; set; } = new();
    public List<string> DislikedIngredients { get; set; } = new();
    public List<string> PreferredCategories { get; set; } = new();
}