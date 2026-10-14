using R2D2.Domain.Enums;

namespace R2D2.Domain.Entities;

public class DynamicField
{
    public Guid Id { get; set; }
    public string TechnicalName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public int? MaxLength { get; set; } // For Text → VARCHAR(n)
    public Guid ListId { get; set; }
    public Guid? RelatedListId { get; set; } // For Relationship type
    public string? RelatedFieldTechnicalName { get; set; } // Field from the related list
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public DynamicList List { get; set; } = null!;
    public DynamicList? RelatedList { get; set; }
    public ICollection<FieldValue> FieldValues { get; set; } = new List<FieldValue>();
}
