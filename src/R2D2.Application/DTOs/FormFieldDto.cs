using R2D2.Domain.Enums;

namespace R2D2.Application.DTOs;

public record FormFieldDto(
    Guid Id,
    Guid FormId,
    string Label,
    FormFieldType FieldType,
    int Order,
    int ColumnSize,
    bool IsReadOnly,
    Guid? LinkedListId,
    string? LinkedListName,
    Guid? LinkedFieldId,
    string? LinkedFieldName,
    Guid? ParentFormFieldId,
    string? CascadeFilterField,
    Guid? SelectSourceListId,
    string? SelectValueField,
    string? SelectDisplayField,
    string? SelectSourceFilterField,
    string? SelectSourceFilterValue
);
