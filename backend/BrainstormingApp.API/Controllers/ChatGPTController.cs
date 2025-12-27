using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using BrainstormingApp.Application.Common;
using BrainstormingApp.Application.DTOs.ChatGPT;
using BrainstormingApp.Application.Interfaces;

namespace BrainstormingApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("chatgpt")]
public class ChatGPTController : ControllerBase
{
    private readonly IChatGPTService _chatGPTService;
    private readonly IReportingService _reportingService;

    public ChatGPTController(IChatGPTService chatGPTService, IReportingService reportingService)
    {
        _chatGPTService = chatGPTService;
        _reportingService = reportingService;
    }

    /// <summary>
    /// Generate ideas using AI
    /// </summary>
    [HttpPost("generate-ideas")]
    public async Task<ActionResult<ApiResponse<GenerateIdeasResponseDto>>> GenerateIdeas([FromBody] GenerateIdeasRequestDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _chatGPTService.GenerateIdeasAsync(dto, userId);

        // Log action
        await _reportingService.LogActionAsync(dto.SessionId, userId, "AIIdeasGenerated",
            $"Generated {result.GeneratedIdeas.Count} ideas, used {result.TokensUsed} tokens");

        return Ok(ApiResponse<GenerateIdeasResponseDto>.SuccessResponse(result, "Ideas generated successfully"));
    }

    /// <summary>
    /// Generate a summary of all ideas in a session
    /// </summary>
    [HttpPost("generate-summary")]
    public async Task<ActionResult<ApiResponse<GenerateSummaryResponseDto>>> GenerateSummary([FromBody] GenerateSummaryRequestDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _chatGPTService.GenerateSummaryAsync(dto, userId);

        // Log action
        await _reportingService.LogActionAsync(dto.SessionId, userId, "AISummaryGenerated",
            $"Summary generated, used {result.TokensUsed} tokens");

        return Ok(ApiResponse<GenerateSummaryResponseDto>.SuccessResponse(result, "Summary generated successfully"));
    }

    /// <summary>
    /// Generate annotation for a specific idea
    /// </summary>
    [HttpPost("generate-annotation")]
    public async Task<ActionResult<ApiResponse<GenerateAnnotationResponseDto>>> GenerateAnnotation([FromBody] GenerateAnnotationRequestDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _chatGPTService.GenerateAnnotationAsync(dto, userId);
        return Ok(ApiResponse<GenerateAnnotationResponseDto>.SuccessResponse(result, "Annotation generated successfully"));
    }

    /// <summary>
    /// Get all AI interactions for a session
    /// </summary>
    [DisableRateLimiting]
    [HttpGet("session/{sessionId}/interactions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ChatGPTInteractionDto>>>> GetSessionInteractions(Guid sessionId)
    {
        var interactions = await _chatGPTService.GetInteractionsBySessionAsync(sessionId);
        return Ok(ApiResponse<IEnumerable<ChatGPTInteractionDto>>.SuccessResponse(interactions));
    }

    /// <summary>
    /// Get all AI interactions for current user
    /// </summary>
    [DisableRateLimiting]
    [HttpGet("my-interactions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ChatGPTInteractionDto>>>> GetMyInteractions()
    {
        var userId = GetCurrentUserId();
        var interactions = await _chatGPTService.GetInteractionsByUserAsync(userId);
        return Ok(ApiResponse<IEnumerable<ChatGPTInteractionDto>>.SuccessResponse(interactions));
    }

    /// <summary>
    /// Check rate limit status for current user in a session
    /// </summary>
    [DisableRateLimiting]
    [HttpGet("session/{sessionId}/rate-limit")]
    public async Task<ActionResult<ApiResponse<object>>> CheckRateLimit(Guid sessionId)
    {
        var userId = GetCurrentUserId();
        var canRequest = await _chatGPTService.ValidateRateLimitAsync(userId, sessionId);
        var usedCount = await _chatGPTService.GetUserRequestCountAsync(userId, sessionId);
        return Ok(ApiResponse<object>.SuccessResponse(new { canRequest, usedCount, maxAllowed = 10 }));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }

        return userId;
    }
}
