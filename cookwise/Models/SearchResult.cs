namespace cookwise.Models;

public class SearchResult
{
    public Recipe Recipe { get; set; } = null!;
    public List<Ingredient> MissingIngredients { get; set; } = new();
    public int MatchScore { get; set; }
}