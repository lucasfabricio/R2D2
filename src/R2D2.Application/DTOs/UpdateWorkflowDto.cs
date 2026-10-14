namespace R2D2.Application.DTOs;

public record UpdateWorkflowDto(
    string Name,
    string? Description,
    string FlowJson,
    bool IsActive
);
