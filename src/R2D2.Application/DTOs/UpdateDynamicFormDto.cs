using R2D2.Domain.Enums;

namespace R2D2.Application.DTOs;

public record UpdateDynamicFormDto(
    string Name,
    string? Description,
    FormType FormType,
    Guid? WorkflowId
);
