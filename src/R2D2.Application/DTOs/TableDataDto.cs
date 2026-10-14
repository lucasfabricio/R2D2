namespace R2D2.Application.DTOs;

public record TableDataDto(
    Guid Id,
    Guid ListId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Dictionary<string, object?> Values
);
