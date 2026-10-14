namespace R2D2.Application.DTOs;

public record WorkflowDto(
    Guid Id,
    string Name,
    string? Description,
    string FlowJson,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<WorkflowStepDto> Steps
);
