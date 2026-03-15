namespace cookwise.Models;

public class UserNote
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = "default";
    public string RecipeId { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public string AdjustmentsText { get; set; } = string.Empty;
    public string Rating { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}