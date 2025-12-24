using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using BrainstormingApp.API.Hubs;
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
    public async Task<ActionResult<IdeaDto>> Submit([FromBody] CreateIdeaDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var idea = await _ideaService.SubmitIdeaAsync(dto, userId);

            // Log action
            await _reportingService.LogActionAsync(dto.SessionId, userId, "IdeaSubmitted",
                $"Idea #{idea.OrderInRound} in round {idea.RoundNumber}");

            // Notify connected clients
            await _hubContext.Clients.Group($"session_{dto.SessionId}").SendAsync("IdeaSubmitted", idea);

            return CreatedAtAction(nameof(GetByRound), new { roundId = idea.RoundId }, idea);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing idea (only during active round)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<IdeaDto>> Update(Guid id, [FromBody] UpdateIdeaDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var idea = await _ideaService.UpdateIdeaAsync(id, dto, userId);

            // Notify connected clients
            await _hubContext.Clients.Group($"session_{idea.SessionId}").SendAsync("IdeaUpdated", idea);

            return Ok(idea);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all ideas for a round
    /// </summary>
    [HttpGet("round/{roundId}")]
    public async Task<ActionResult<IEnumerable<IdeaDto>>> GetByRound(Guid roundId)
    {
        try
        {
            var ideas = await _ideaService.GetIdeasByRoundAsync(roundId);
            return Ok(ideas);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all ideas for a session
    /// </summary>
    [HttpGet("session/{sessionId}")]
    public async Task<ActionResult<IEnumerable<IdeaDto>>> GetBySession(Guid sessionId)
    {
        try
        {
            var ideas = await _ideaService.GetIdeasBySessionAsync(sessionId);
            return Ok(ideas);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get ideas grouped by round for a session
    /// </summary>
    [HttpGet("session/{sessionId}/grouped")]
    public async Task<ActionResult<IEnumerable<IdeasByRoundDto>>> GetGroupedByRound(Guid sessionId)
    {
        try
        {
            var groupedIdeas = await _ideaService.GetIdeasGroupedByRoundAsync(sessionId);
            return Ok(groupedIdeas);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get current user's ideas in a round
    /// </summary>
    [HttpGet("round/{roundId}/my-ideas")]
    public async Task<ActionResult<IEnumerable<IdeaDto>>> GetMyIdeasInRound(Guid roundId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var ideas = await _ideaService.GetUserIdeasInRoundAsync(roundId, userId);
            return Ok(ideas);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Check if current user can submit more ideas in a round
    /// </summary>
    [HttpGet("round/{roundId}/can-submit")]
    public async Task<ActionResult> CanSubmit(Guid roundId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var canSubmit = await _ideaService.ValidateIdeaLimitAsync(roundId, userId);
            var currentCount = await _ideaService.GetUserIdeaCountInRoundAsync(roundId, userId);
            return Ok(new { canSubmit, currentCount, maxAllowed = 3 });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Check if user can submit to a session (is member and limit not reached)
    /// </summary>
    [HttpGet("session/{sessionId}/can-submit")]
    public async Task<ActionResult> CanSubmitToSession(Guid sessionId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var canSubmit = await _ideaService.CanUserSubmitAsync(sessionId, userId);
            return Ok(new { canSubmit });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete an idea (only during active round)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _ideaService.DeleteIdeaAsync(id, userId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
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
