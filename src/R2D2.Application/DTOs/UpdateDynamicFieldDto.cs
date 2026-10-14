using R2D2.Domain.Enums;

namespace R2D2.Application.DTOs;

public record UpdateDynamicFieldDto(
    string TechnicalName,
    string Label,
    FieldType FieldType,
    bool IsRequired,
    int? MaxLength,
    Guid? RelatedListId,
    string? RelatedFieldTechnicalName
);
