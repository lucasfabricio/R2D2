namespace R2D2.Domain.Entities;

public class FieldValue
{
    public Guid Id { get; set; }
    public Guid TableDataId { get; set; }
    public Guid FieldId { get; set; }
    public string? Value { get; set; } // Stored as string, parsed based on FieldType

    // Navigation properties
    public TableData TableData { get; set; } = null!;
    public DynamicField Field { get; set; } = null!;
}
