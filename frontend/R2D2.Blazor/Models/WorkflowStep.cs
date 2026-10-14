namespace R2D2.Blazor.Models;

public class WorkflowStep
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public Guid RuleId { get; set; }
    public string RuleName { get; set; } = "";
    public int Order { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public string? NextStepOnSuccess { get; set; }
    public string? NextStepOnFailure { get; set; }
}
