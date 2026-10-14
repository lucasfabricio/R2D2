namespace R2D2.Blazor.Models;

public class DynamicList
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<DynamicField> Fields { get; set; } = new();
}
