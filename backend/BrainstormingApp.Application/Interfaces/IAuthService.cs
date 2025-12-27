using BrainstormingApp.Application.DTOs.Auth;

namespace BrainstormingApp.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto?> GetCurrentUserAsync(Guid userId);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    string GenerateJwtToken(Guid userId, string email, string role);
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
