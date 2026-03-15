namespace cookwise.Models;

public class CookingStep
{
    public int StepNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? DurationMinutes { get; set; }
    public string? HeatLevel { get; set; }
    public List<Ingredient> Ingredients { get; set; } = new();
}