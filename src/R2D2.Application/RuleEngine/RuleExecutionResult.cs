namespace R2D2.Application.RuleEngine;

public class RuleExecutionResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}
