namespace R2D2.Blazor.Models;

public class ListView
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? DisplayName { get; set; }
    public Guid ListId { get; set; }
    public string ListName { get; set; } = "";
    public Guid? CreateFormId { get; set; }
    public string? CreateFormName { get; set; }
    public Guid? UpdateFormId { get; set; }
    public string? UpdateFormName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ListViewColumn> Columns { get; set; } = new();
}

public class ListViewColumn
{
    public Guid Id { get; set; }
    public Guid ListViewId { get; set; }
    public string FieldTechnicalName { get; set; } = "";
    public int Order { get; set; }
    public string? HeaderLabel { get; set; }
}
