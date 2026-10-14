namespace R2D2.Domain.Entities;

public class DynamicList
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<DynamicField> Fields { get; set; } = new List<DynamicField>();
    public ICollection<TableData> TableData { get; set; } = new List<TableData>();
}
