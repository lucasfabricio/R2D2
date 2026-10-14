namespace R2D2.Application.RuleEngine;

public class RuleExecutionContext
{
    public Dictionary<string, object?> Variables { get; }
    public Dictionary<string, RuleExecutionResult> StepResults { get; } = new();

    public RuleExecutionContext(Dictionary<string, object?> initialVariables)
    {
        Variables = new Dictionary<string, object?>(initialVariables);
    }

    public void SetVariable(string key, object? value) => Variables[key] = value;
    public object? GetVariable(string key) => Variables.TryGetValue(key, out var value) ? value : null;
    public void AddStepResult(string stepId, RuleExecutionResult result) => StepResults[stepId] = result;
}
