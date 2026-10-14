namespace R2D2.Application.DTOs;

public record DynamicListDto(
    Guid Id,
    string Name,
    string? DisplayName,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<DynamicFieldDto> Fields
);
