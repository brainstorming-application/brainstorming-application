using BrainstormingApp.Application.DTOs.Auth;

namespace BrainstormingApp.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto?> GetCurrentUserAsync(Guid userId);
    string GenerateJwtToken(Guid userId, string email, string role);
}
