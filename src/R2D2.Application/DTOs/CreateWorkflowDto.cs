namespace R2D2.Application.DTOs;

public record CreateWorkflowDto(
    string Name,
    string? Description,
    string FlowJson,
    bool IsActive = true,
    List<CreateWorkflowStepDto>? Steps = null
);
