using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using BrainstormingApp.API.Hubs;
using BrainstormingApp.Application.DTOs.Session;
using BrainstormingApp.Application.Interfaces;

namespace BrainstormingApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IReportingService _reportingService;
    private readonly IHubContext<BrainstormingHub> _hubContext;

    public SessionsController(
        ISessionService sessionService,
        IReportingService reportingService,
        IHubContext<BrainstormingHub> hubContext)
    {
        _sessionService = sessionService;
        _reportingService = reportingService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Get all sessions for a team
    /// </summary>
    [HttpGet("team/{teamId}")]
    public async Task<ActionResult<IEnumerable<SessionDto>>> GetByTeam(Guid teamId)
    {
        try
        {
            var sessions = await _sessionService.GetSessionsByTeamAsync(teamId);
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all sessions for a topic
    /// </summary>
    [HttpGet("topic/{topicId}")]
    public async Task<ActionResult<IEnumerable<SessionDto>>> GetByTopic(Guid topicId)
    {
        try
        {
            var sessions = await _sessionService.GetSessionsByTopicAsync(topicId);
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific session by ID with full details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SessionDetailDto>> GetById(Guid id)
    {
        try
        {
            var session = await _sessionService.GetSessionByIdAsync(id);
            if (session == null)
            {
                return NotFound(new { message = "Session not found" });
            }
            return Ok(session);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new brainstorming session
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SessionDto>> Create([FromBody] CreateSessionDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var session = await _sessionService.CreateSessionAsync(dto, userId);

            // Log action
            await _reportingService.LogActionAsync(session.Id, userId, "SessionCreated",
                $"Session created for team {session.TeamName} on topic {session.TopicTitle}");

            return CreatedAtAction(nameof(GetById), new { id = session.Id }, session);
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
    /// Start a brainstorming session
    /// </summary>
    [HttpPost("{id}/start")]
    public async Task<ActionResult<SessionDto>> Start(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var session = await _sessionService.StartSessionAsync(id, userId);

            // Log action
            await _reportingService.LogActionAsync(id, userId, "SessionStarted",
                $"Round {session.CurrentRound} started");

            // Notify connected clients
            await _hubContext.Clients.Group($"session_{id}").SendAsync("SessionStarted", session);
            await _hubContext.Clients.Group($"session_{id}").SendAsync("RoundStarted", session.CurrentRound);

            return Ok(session);
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
    /// Pause a brainstorming session
    /// </summary>
    [HttpPost("{id}/pause")]
    public async Task<ActionResult<SessionDto>> Pause(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var session = await _sessionService.PauseSessionAsync(id, userId);

            // Log action
            await _reportingService.LogActionAsync(id, userId, "SessionPaused", null);

            // Notify connected clients
            await _hubContext.Clients.Group($"session_{id}").SendAsync("SessionPaused", session);

            return Ok(session);
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
    /// Resume a paused session
    /// </summary>
    [HttpPost("{id}/resume")]
    public async Task<ActionResult<SessionDto>> Resume(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var session = await _sessionService.ResumeSessionAsync(id, userId);

            // Log action
            await _reportingService.LogActionAsync(id, userId, "SessionResumed", null);

            // Notify connected clients
            await _hubContext.Clients.Group($"session_{id}").SendAsync("SessionResumed", session);

            return Ok(session);
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
    /// End a brainstorming session
    /// </summary>
    [HttpPost("{id}/end")]
    public async Task<ActionResult<SessionDto>> End(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var session = await _sessionService.EndSessionAsync(id, userId);

            // Log action
            await _reportingService.LogActionAsync(id, userId, "SessionEnded",
                $"Session completed with {session.TotalIdeas} ideas");

            // Notify connected clients
            await _hubContext.Clients.Group($"session_{id}").SendAsync("SessionCompleted", session);

            return Ok(session);
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
    /// Advance to next round
    /// </summary>
    [HttpPost("{id}/advance-round")]
    public async Task<ActionResult<RoundDto>> AdvanceRound(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var round = await _sessionService.AdvanceRoundAsync(id, userId);

            // Log action
            await _reportingService.LogActionAsync(id, userId, "RoundAdvanced",
                $"Advanced to round {round.RoundNumber}");

            // Notify connected clients
            await _hubContext.Clients.Group($"session_{id}").SendAsync("RoundEnded", round.RoundNumber - 1);
            await _hubContext.Clients.Group($"session_{id}").SendAsync("RoundStarted", round.RoundNumber);

            return Ok(round);
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
    /// Get current round info
    /// </summary>
    [HttpGet("{id}/current-round")]
    public async Task<ActionResult<RoundDto>> GetCurrentRound(Guid id)
    {
        try
        {
            var round = await _sessionService.GetCurrentRoundAsync(id);
            if (round == null)
            {
                return NotFound(new { message = "No active round" });
            }
            return Ok(round);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get remaining time in current round (in seconds)
    /// </summary>
    [HttpGet("{id}/remaining-time")]
    public async Task<ActionResult<int>> GetRemainingTime(Guid id)
    {
        try
        {
            var remainingSeconds = await _sessionService.GetRemainingTimeAsync(id);
            return Ok(new { remainingSeconds });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get session status
    /// </summary>
    [HttpGet("{id}/status")]
    public async Task<ActionResult> GetStatus(Guid id)
    {
        try
        {
            var status = await _sessionService.GetSessionStatusAsync(id);
            return Ok(new { status = status.ToString() });
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
