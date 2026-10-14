using R2D2.Domain.Enums;

namespace R2D2.Application.DTOs;

public record CreateDynamicFormDto(
    string Name,
    string? Description,
    FormType FormType,
    Guid? WorkflowId,
    List<CreateFormFieldDto>? Fields
);
