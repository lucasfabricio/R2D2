namespace R2D2.Application.DTOs;

public record WorkflowStepDto(
    Guid Id,
    Guid WorkflowId,
    Guid RuleId,
    string RuleName,
    int Order,
    int PositionX,
    int PositionY,
    string? NextStepOnSuccess,
    string? NextStepOnFailure
);
