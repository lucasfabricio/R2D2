namespace R2D2.Domain.Entities;

public class WorkflowStep
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public Guid RuleId { get; set; }
    public int Order { get; set; }
    public int PositionX { get; set; } // Visual position X
    public int PositionY { get; set; } // Visual position Y
    public string? NextStepOnSuccess { get; set; } // Step ID JSON array
    public string? NextStepOnFailure { get; set; } // Step ID JSON array

    // Navigation properties
    public Workflow Workflow { get; set; } = null!;
    public BusinessRule Rule { get; set; } = null!;
}
