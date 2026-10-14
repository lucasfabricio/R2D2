namespace R2D2.Application.DTOs;

public record CreateTableDataDto(
    Guid ListId,
    Dictionary<string, object?> Values
);
