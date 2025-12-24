using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BrainstormingApp.Application.DTOs.ChatGPT;
using BrainstormingApp.Application.Interfaces;

namespace BrainstormingApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
    public async Task<ActionResult<GenerateIdeasResponseDto>> GenerateIdeas([FromBody] GenerateIdeasRequestDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _chatGPTService.GenerateIdeasAsync(dto, userId);

            // Log action
            await _reportingService.LogActionAsync(dto.SessionId, userId, "AIIdeasGenerated",
                $"Generated {result.GeneratedIdeas.Count} ideas, used {result.TokensUsed} tokens");

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Generate a summary of all ideas in a session
    /// </summary>
    [HttpPost("generate-summary")]
    public async Task<ActionResult<GenerateSummaryResponseDto>> GenerateSummary([FromBody] GenerateSummaryRequestDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _chatGPTService.GenerateSummaryAsync(dto, userId);

            // Log action
            await _reportingService.LogActionAsync(dto.SessionId, userId, "AISummaryGenerated",
                $"Summary generated, used {result.TokensUsed} tokens");

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Generate annotation for a specific idea
    /// </summary>
    [HttpPost("generate-annotation")]
    public async Task<ActionResult<GenerateAnnotationResponseDto>> GenerateAnnotation([FromBody] GenerateAnnotationRequestDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _chatGPTService.GenerateAnnotationAsync(dto, userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all AI interactions for a session
    /// </summary>
    [HttpGet("session/{sessionId}/interactions")]
    public async Task<ActionResult<IEnumerable<ChatGPTInteractionDto>>> GetSessionInteractions(Guid sessionId)
    {
        try
        {
            var interactions = await _chatGPTService.GetInteractionsBySessionAsync(sessionId);
            return Ok(interactions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all AI interactions for current user
    /// </summary>
    [HttpGet("my-interactions")]
    public async Task<ActionResult<IEnumerable<ChatGPTInteractionDto>>> GetMyInteractions()
    {
        try
        {
            var userId = GetCurrentUserId();
            var interactions = await _chatGPTService.GetInteractionsByUserAsync(userId);
            return Ok(interactions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Check rate limit status for current user in a session
    /// </summary>
    [HttpGet("session/{sessionId}/rate-limit")]
    public async Task<ActionResult> CheckRateLimit(Guid sessionId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var canRequest = await _chatGPTService.ValidateRateLimitAsync(userId, sessionId);
            var usedCount = await _chatGPTService.GetUserRequestCountAsync(userId, sessionId);
            return Ok(new { canRequest, usedCount, maxAllowed = 10 });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
