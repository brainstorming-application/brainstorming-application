using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using BrainstormingApp.Application.Common;
using BrainstormingApp.Application.DTOs.Auth;
using BrainstormingApp.Application.Interfaces;

namespace BrainstormingApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto registerDto)
    {
        var response = await _authService.RegisterAsync(registerDto);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response, "Registration successful"));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
    {
        var response = await _authService.LoginAsync(loginDto);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response, "Login successful"));
    }

    [Authorize]
    [DisableRateLimiting]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new Application.Common.Exceptions.UnauthorizedException("Invalid token");
        }

        var response = await _authService.GetCurrentUserAsync(userId);
        if (response == null)
        {
            throw new Application.Common.Exceptions.NotFoundException("User", userId);
        }

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response));
    }
}
