namespace R2D2.Blazor.Models;

public class TableData
{
    public Guid Id { get; set; }
    public Guid ListId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Dictionary<string, object?> Values { get; set; } = new();
}
