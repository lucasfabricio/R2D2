using R2D2.Domain.Enums;

namespace R2D2.Domain.Entities;

public class DynamicForm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public FormType FormType { get; set; } = FormType.Create;
    public Guid? WorkflowId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Workflow? Workflow { get; set; }
    public ICollection<FormField> Fields { get; set; } = new List<FormField>();
}
