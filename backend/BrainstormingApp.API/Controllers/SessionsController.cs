using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using BrainstormingApp.API.Hubs;
using BrainstormingApp.Application.Common;
using BrainstormingApp.Application.Common.Exceptions;
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
    public async Task<ActionResult<ApiResponse<IEnumerable<SessionDto>>>> GetByTeam(Guid teamId)
    {
        var sessions = await _sessionService.GetSessionsByTeamAsync(teamId);
        return Ok(ApiResponse<IEnumerable<SessionDto>>.SuccessResponse(sessions));
    }

    /// <summary>
    /// Get all sessions for a topic
    /// </summary>
    [HttpGet("topic/{topicId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SessionDto>>>> GetByTopic(Guid topicId)
    {
        var sessions = await _sessionService.GetSessionsByTopicAsync(topicId);
        return Ok(ApiResponse<IEnumerable<SessionDto>>.SuccessResponse(sessions));
    }

    /// <summary>
    /// Get a specific session by ID with full details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SessionDetailDto>>> GetById(Guid id)
    {
        var session = await _sessionService.GetSessionByIdAsync(id);
        if (session == null)
        {
            throw new NotFoundException("Session", id);
        }
        return Ok(ApiResponse<SessionDetailDto>.SuccessResponse(session));
    }

    /// <summary>
    /// Create a new brainstorming session
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<SessionDto>>> Create([FromBody] CreateSessionDto dto)
    {
        var userId = GetCurrentUserId();
        var session = await _sessionService.CreateSessionAsync(dto, userId);

        // Log action
        await _reportingService.LogActionAsync(session.Id, userId, "SessionCreated",
            $"Session created for team {session.TeamName} on topic {session.TopicTitle}");

        return CreatedAtAction(nameof(GetById), new { id = session.Id },
            ApiResponse<SessionDto>.SuccessResponse(session, "Session created successfully"));
    }

    /// <summary>
    /// Start a brainstorming session
    /// </summary>
    [HttpPost("{id}/start")]
    public async Task<ActionResult<ApiResponse<SessionDto>>> Start(Guid id)
    {
        var userId = GetCurrentUserId();
        var session = await _sessionService.StartSessionAsync(id, userId);

        // Log action
        await _reportingService.LogActionAsync(id, userId, "SessionStarted",
            $"Round {session.CurrentRound} started");

        // Notify connected clients
        await _hubContext.Clients.Group($"session_{id}").SendAsync("SessionStarted", session);
        await _hubContext.Clients.Group($"session_{id}").SendAsync("RoundStarted", session.CurrentRound);

        return Ok(ApiResponse<SessionDto>.SuccessResponse(session, "Session started successfully"));
    }

    /// <summary>
    /// Pause a brainstorming session
    /// </summary>
    [HttpPost("{id}/pause")]
    public async Task<ActionResult<ApiResponse<SessionDto>>> Pause(Guid id)
    {
        var userId = GetCurrentUserId();
        var session = await _sessionService.PauseSessionAsync(id, userId);

        // Log action
        await _reportingService.LogActionAsync(id, userId, "SessionPaused", null);

        // Notify connected clients
        await _hubContext.Clients.Group($"session_{id}").SendAsync("SessionPaused", session);

        return Ok(ApiResponse<SessionDto>.SuccessResponse(session, "Session paused"));
    }

    /// <summary>
    /// Resume a paused session
    /// </summary>
    [HttpPost("{id}/resume")]
    public async Task<ActionResult<ApiResponse<SessionDto>>> Resume(Guid id)
    {
        var userId = GetCurrentUserId();
        var session = await _sessionService.ResumeSessionAsync(id, userId);

        // Log action
        await _reportingService.LogActionAsync(id, userId, "SessionResumed", null);

        // Notify connected clients
        await _hubContext.Clients.Group($"session_{id}").SendAsync("SessionResumed", session);

        return Ok(ApiResponse<SessionDto>.SuccessResponse(session, "Session resumed"));
    }

    /// <summary>
    /// End a brainstorming session
    /// </summary>
    [HttpPost("{id}/end")]
    public async Task<ActionResult<ApiResponse<SessionDto>>> End(Guid id)
    {
        var userId = GetCurrentUserId();
        var session = await _sessionService.EndSessionAsync(id, userId);

        // Log action
        await _reportingService.LogActionAsync(id, userId, "SessionEnded",
            $"Session completed with {session.TotalIdeas} ideas");

        // Notify connected clients
        await _hubContext.Clients.Group($"session_{id}").SendAsync("SessionCompleted", session);

        return Ok(ApiResponse<SessionDto>.SuccessResponse(session, "Session completed successfully"));
    }

    /// <summary>
    /// Advance to next round
    /// </summary>
    [HttpPost("{id}/advance-round")]
    public async Task<ActionResult<ApiResponse<RoundDto>>> AdvanceRound(Guid id)
    {
        var userId = GetCurrentUserId();
        var round = await _sessionService.AdvanceRoundAsync(id, userId);

        // Log action
        await _reportingService.LogActionAsync(id, userId, "RoundAdvanced",
            $"Advanced to round {round.RoundNumber}");

        // Notify connected clients
        await _hubContext.Clients.Group($"session_{id}").SendAsync("RoundEnded", round.RoundNumber - 1);
        await _hubContext.Clients.Group($"session_{id}").SendAsync("RoundStarted", round.RoundNumber);

        return Ok(ApiResponse<RoundDto>.SuccessResponse(round, $"Advanced to round {round.RoundNumber}"));
    }

    /// <summary>
    /// Get current round info
    /// </summary>
    [HttpGet("{id}/current-round")]
    public async Task<ActionResult<ApiResponse<RoundDto>>> GetCurrentRound(Guid id)
    {
        var round = await _sessionService.GetCurrentRoundAsync(id);
        if (round == null)
        {
            throw new NotFoundException("No active round found");
        }
        return Ok(ApiResponse<RoundDto>.SuccessResponse(round));
    }

    /// <summary>
    /// Get remaining time in current round (in seconds)
    /// </summary>
    [HttpGet("{id}/remaining-time")]
    public async Task<ActionResult<ApiResponse<object>>> GetRemainingTime(Guid id)
    {
        var remainingSeconds = await _sessionService.GetRemainingTimeAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(new { remainingSeconds }));
    }

    /// <summary>
    /// Get session status
    /// </summary>
    [HttpGet("{id}/status")]
    public async Task<ActionResult<ApiResponse<object>>> GetStatus(Guid id)
    {
        var status = await _sessionService.GetSessionStatusAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(new { status = status.ToString() }));
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
