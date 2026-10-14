using R2D2.Domain.Enums;

namespace R2D2.Domain.Entities;

public class FormField
{
    public Guid Id { get; set; }
    public Guid FormId { get; set; }
    public string Label { get; set; } = string.Empty;
    public FormFieldType FieldType { get; set; }
    public int Order { get; set; }
    public int ColumnSize { get; set; } = 12; // Grid 1-12
    public bool IsReadOnly { get; set; }

    // Optional binding to a list field (for data persistence)
    public Guid? LinkedListId { get; set; }
    public Guid? LinkedFieldId { get; set; }

    // Cascade (only for Select type)
    public Guid? ParentFormFieldId { get; set; }
    public string? CascadeFilterField { get; set; }

    // Select field configuration (value/display columns from source list)
    public Guid? SelectSourceListId { get; set; }    // List used to populate select options
    public string? SelectValueField { get; set; }   // Technical name of the column to use as <option value>
    public string? SelectDisplayField { get; set; } // Technical name of the column to display
    public string? SelectSourceFilterField { get; set; }  // Field from source list to filter by
    public string? SelectSourceFilterValue { get; set; }  // Constant value to match

    // Navigation properties
    public DynamicForm Form { get; set; } = null!;
    public DynamicList? LinkedList { get; set; }
    public DynamicField? LinkedField { get; set; }
    public FormField? ParentFormField { get; set; }
    public ICollection<FormField> ChildFormFields { get; set; } = new List<FormField>();
}
