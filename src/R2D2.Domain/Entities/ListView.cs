namespace R2D2.Domain.Entities;

public class ListView
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public Guid ListId { get; set; }
    public Guid? CreateFormId { get; set; }
    public Guid? UpdateFormId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public DynamicList List { get; set; } = null!;
    public DynamicForm? CreateForm { get; set; }
    public DynamicForm? UpdateForm { get; set; }
    public ICollection<ListViewColumn> Columns { get; set; } = new List<ListViewColumn>();
}
