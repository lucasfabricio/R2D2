namespace R2D2.Blazor.Models;

public class Workflow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string FlowJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<WorkflowStep> Steps { get; set; } = new();
}
