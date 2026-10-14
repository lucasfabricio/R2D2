using R2D2.Domain.Enums;

namespace R2D2.Application.DTOs;

public record FieldValueDto(
    Guid Id,
    Guid FieldId,
    string FieldName,
    FieldType FieldType,
    string? Value
);
