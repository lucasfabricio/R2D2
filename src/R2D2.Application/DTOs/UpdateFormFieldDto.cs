using R2D2.Domain.Enums;

namespace R2D2.Application.DTOs;

public record UpdateFormFieldDto(
    string Label,
    FormFieldType FieldType,
    int Order,
    int ColumnSize,
    bool IsReadOnly,
    Guid? LinkedListId,
    Guid? LinkedFieldId,
    Guid? ParentFormFieldId,
    string? CascadeFilterField,
    Guid? SelectSourceListId,
    string? SelectValueField,
    string? SelectDisplayField,
    string? SelectSourceFilterField,
    string? SelectSourceFilterValue
);
