namespace R2D2.Blazor.Models;

public class DynamicField
{
    public Guid Id { get; set; }
    public Guid ListId { get; set; }
    public string TechnicalName { get; set; } = "";
    public string Label { get; set; } = "";
    public FieldType FieldType { get; set; } = FieldType.Text;
    public bool IsRequired { get; set; }
    public int? MaxLength { get; set; }
    public Guid? RelatedListId { get; set; }
    public string? RelatedFieldTechnicalName { get; set; }
    public string? SelectOptions { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
