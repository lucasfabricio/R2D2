namespace R2D2.Application.DTOs;

public record AuthResponseDto(
    string Token,
    string Username,
    DateTime ExpiresAt
);
