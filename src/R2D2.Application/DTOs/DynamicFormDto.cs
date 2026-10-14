using R2D2.Domain.Enums;

namespace R2D2.Application.DTOs;

public record DynamicFormDto(
    Guid Id,
    string Name,
    string? Description,
    FormType FormType,
    Guid? WorkflowId,
    string? WorkflowName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<FormFieldDto> Fields
);
