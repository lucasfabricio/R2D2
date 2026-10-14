namespace R2D2.Application.DTOs;

public record UpdateWorkflowStepDto(
    Guid RuleId,
    int Order,
    int PositionX,
    int PositionY,
    string? NextStepOnSuccess,
    string? NextStepOnFailure
);
