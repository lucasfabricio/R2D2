namespace R2D2.Blazor.Models;

public class FormField
{
    public Guid Id { get; set; }
    public Guid FormId { get; set; }
    public string Label { get; set; } = "";
    public FormFieldType FieldType { get; set; }
    public int Order { get; set; }
    public int ColumnSize { get; set; } = 12;
    public bool IsReadOnly { get; set; }
    public Guid? LinkedListId { get; set; }
    public string? LinkedListName { get; set; }
    public Guid? LinkedFieldId { get; set; }
    public string? LinkedFieldName { get; set; }
    public Guid? ParentFormFieldId { get; set; }
    public string? CascadeFilterField { get; set; }
    public Guid? SelectSourceListId { get; set; }
    public string? SelectValueField { get; set; }
    public string? SelectDisplayField { get; set; }
    public string? SelectSourceFilterField { get; set; }
    public string? SelectSourceFilterValue { get; set; }
}
