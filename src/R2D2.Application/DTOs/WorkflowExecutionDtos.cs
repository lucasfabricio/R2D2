using R2D2.Application.RuleEngine;

namespace R2D2.Application.DTOs;

public record WorkflowExecutionResultDto(
    bool Success,
    Guid WorkflowId,
    List<string> Errors,
    Dictionary<string, RuleExecutionResult> StepResults
);
