using R2D2.Domain.Enums;

namespace R2D2.Domain.Entities;

public class BusinessRule
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RuleType Type { get; set; }
    public string ConfigurationJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<WorkflowStep> WorkflowSteps { get; set; } = new List<WorkflowStep>();
}
