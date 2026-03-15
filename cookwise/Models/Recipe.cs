namespace cookwise.Models;

public class Recipe
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Ingredient> Ingredients { get; set; } = new();
    public List<CookingStep> Steps { get; set; } = new();
    public int Servings { get; set; } = 1;
    public double EstimatedCost { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public List<string> CategoryTags { get; set; } = new();
    public bool IsCustom { get; set; }
    public string? ParentRecipeId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}