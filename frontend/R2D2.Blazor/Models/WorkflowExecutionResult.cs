namespace R2D2.Blazor.Models;

public class WorkflowExecutionResult
{
    public bool Success { get; set; }
    public Guid WorkflowId { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, StepResult> StepResults { get; set; } = new();
}

public class StepResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
}
