namespace R2D2.Application.DTOs;

public record CreateDynamicListDto(
    string Name,
    string? DisplayName,
    string? Description,
    List<CreateDynamicFieldDto>? Fields
);
