namespace R2D2.Application.DTOs;

public record UpdateDynamicListDto(
    string Name,
    string? DisplayName,
    string? Description
);
