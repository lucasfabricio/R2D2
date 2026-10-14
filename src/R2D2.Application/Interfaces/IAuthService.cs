using R2D2.Application.DTOs;

namespace R2D2.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<UserDto> RegisterAsync(RegisterDto dto);
    Task<UserDto?> GetCurrentUserAsync(Guid userId);
}
