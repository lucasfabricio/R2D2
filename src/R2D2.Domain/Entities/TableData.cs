namespace R2D2.Domain.Entities;

public class TableData
{
    public Guid Id { get; set; }
    public Guid ListId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public DynamicList List { get; set; } = null!;
    public ICollection<FieldValue> Values { get; set; } = new List<FieldValue>();
}
