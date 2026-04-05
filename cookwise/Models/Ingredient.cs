namespace cookwise.Models;

public class Ingredient
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public double Grams { get; set; }
    public string Category { get; set; } = string.Empty;
    public string IconUrl { get; set; } = "🥬"; // using emoji as a simple placeholder icon
    public List<string> Tags { get; set; } = new();
}