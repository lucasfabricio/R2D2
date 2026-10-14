namespace R2D2.Application.DTOs;

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);
