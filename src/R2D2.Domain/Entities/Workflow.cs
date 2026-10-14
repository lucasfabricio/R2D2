namespace R2D2.Domain.Entities;

public class Workflow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FlowJson { get; set; } = "{}"; // Visual flow structure
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
}
