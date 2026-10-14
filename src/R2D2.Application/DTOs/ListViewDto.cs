namespace R2D2.Application.DTOs;

public record ListViewDto(
    Guid Id,
    string Name,
    string? DisplayName,
    Guid ListId,
    string ListName,
    Guid? CreateFormId,
    string? CreateFormName,
    Guid? UpdateFormId,
    string? UpdateFormName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ListViewColumnDto> Columns
);

public record ListViewColumnDto(
    Guid Id,
    Guid ListViewId,
    string FieldTechnicalName,
    int Order,
    string? HeaderLabel
);

public record CreateListViewDto(
    string Name,
    string? DisplayName,
    Guid ListId,
    Guid? CreateFormId,
    Guid? UpdateFormId,
    List<CreateListViewColumnDto>? Columns
);

public record CreateListViewColumnDto(
    string FieldTechnicalName,
    int Order,
    string? HeaderLabel
);

public record UpdateListViewDto(
    string Name,
    string? DisplayName,
    Guid ListId,
    Guid? CreateFormId,
    Guid? UpdateFormId
);
