using R2D2.Domain.Enums;

namespace R2D2.Application.DTOs;

public record DynamicFieldDto(
    Guid Id,
    Guid ListId,
    string TechnicalName,
    string Label,
    FieldType FieldType,
    bool IsRequired,
    int? MaxLength,
    Guid? RelatedListId,
    string? RelatedFieldTechnicalName,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
