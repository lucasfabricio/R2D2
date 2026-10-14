namespace R2D2.Blazor.Models;

public class DynamicForm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int FormType { get; set; } // 0=Create, 1=Update
    public Guid? WorkflowId { get; set; }
    public string? WorkflowName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<FormField> Fields { get; set; } = new();
}
