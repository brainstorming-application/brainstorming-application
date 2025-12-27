using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using BrainstormingApp.API.Hubs;
using BrainstormingApp.Application.Common;
using BrainstormingApp.Application.Common.Exceptions;
using BrainstormingApp.Application.DTOs.Idea;
using BrainstormingApp.Application.Interfaces;

namespace BrainstormingApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IdeasController : ControllerBase
{
    private readonly IIdeaService _ideaService;
    private readonly IReportingService _reportingService;
    private readonly IHubContext<BrainstormingHub> _hubContext;

    public IdeasController(
        IIdeaService ideaService,
        IReportingService reportingService,
        IHubContext<BrainstormingHub> hubContext)
    {
        _ideaService = ideaService;
        _reportingService = reportingService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Submit a new idea
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<IdeaDto>>> Submit([FromBody] CreateIdeaDto dto)
    {
        var userId = GetCurrentUserId();
        var idea = await _ideaService.SubmitIdeaAsync(dto, userId);

        // Log action
        await _reportingService.LogActionAsync(dto.SessionId, userId, "IdeaSubmitted",
            $"Idea #{idea.OrderInRound} in round {idea.RoundNumber}");

        // Notify connected clients
        await _hubContext.Clients.Group($"session_{dto.SessionId}").SendAsync("IdeaSubmitted", idea);

        return CreatedAtAction(nameof(GetByRound), new { roundId = idea.RoundId },
            ApiResponse<IdeaDto>.SuccessResponse(idea, "Idea submitted successfully"));
    }

    /// <summary>
    /// Update an existing idea (only during active round)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<IdeaDto>>> Update(Guid id, [FromBody] UpdateIdeaDto dto)
    {
        var userId = GetCurrentUserId();
        var idea = await _ideaService.UpdateIdeaAsync(id, dto, userId);

        // Notify connected clients
        await _hubContext.Clients.Group($"session_{idea.SessionId}").SendAsync("IdeaUpdated", idea);

        return Ok(ApiResponse<IdeaDto>.SuccessResponse(idea, "Idea updated successfully"));
    }

    /// <summary>
    /// Get all ideas for a round
    /// </summary>
    [HttpGet("round/{roundId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<IdeaDto>>>> GetByRound(Guid roundId)
    {
        var ideas = await _ideaService.GetIdeasByRoundAsync(roundId);
        return Ok(ApiResponse<IEnumerable<IdeaDto>>.SuccessResponse(ideas));
    }

    /// <summary>
    /// Get all ideas for a session
    /// </summary>
    [HttpGet("session/{sessionId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<IdeaDto>>>> GetBySession(Guid sessionId)
    {
        var ideas = await _ideaService.GetIdeasBySessionAsync(sessionId);
        return Ok(ApiResponse<IEnumerable<IdeaDto>>.SuccessResponse(ideas));
    }

    /// <summary>
    /// Get ideas grouped by round for a session
    /// </summary>
    [HttpGet("session/{sessionId}/grouped")]
    public async Task<ActionResult<ApiResponse<IEnumerable<IdeasByRoundDto>>>> GetGroupedByRound(Guid sessionId)
    {
        var groupedIdeas = await _ideaService.GetIdeasGroupedByRoundAsync(sessionId);
        return Ok(ApiResponse<IEnumerable<IdeasByRoundDto>>.SuccessResponse(groupedIdeas));
    }

    /// <summary>
    /// Get current user's ideas in a round
    /// </summary>
    [HttpGet("round/{roundId}/my-ideas")]
    public async Task<ActionResult<ApiResponse<IEnumerable<IdeaDto>>>> GetMyIdeasInRound(Guid roundId)
    {
        var userId = GetCurrentUserId();
        var ideas = await _ideaService.GetUserIdeasInRoundAsync(roundId, userId);
        return Ok(ApiResponse<IEnumerable<IdeaDto>>.SuccessResponse(ideas));
    }

    /// <summary>
    /// Check if current user can submit more ideas in a round
    /// </summary>
    [HttpGet("round/{roundId}/can-submit")]
    public async Task<ActionResult<ApiResponse<object>>> CanSubmit(Guid roundId)
    {
        var userId = GetCurrentUserId();
        var canSubmit = await _ideaService.ValidateIdeaLimitAsync(roundId, userId);
        var currentCount = await _ideaService.GetUserIdeaCountInRoundAsync(roundId, userId);
        return Ok(ApiResponse<object>.SuccessResponse(new { canSubmit, currentCount, maxAllowed = 3 }));
    }

    /// <summary>
    /// Check if user can submit to a session (is member and limit not reached)
    /// </summary>
    [HttpGet("session/{sessionId}/can-submit")]
    public async Task<ActionResult<ApiResponse<object>>> CanSubmitToSession(Guid sessionId)
    {
        var userId = GetCurrentUserId();
        var canSubmit = await _ideaService.CanUserSubmitAsync(sessionId, userId);
        return Ok(ApiResponse<object>.SuccessResponse(new { canSubmit }));
    }

    /// <summary>
    /// Delete an idea (only during active round)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        await _ideaService.DeleteIdeaAsync(id, userId);
        return Ok(ApiResponse.SuccessResult("Idea deleted successfully"));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Invalid user token");
        }

        return userId;
    }
}
