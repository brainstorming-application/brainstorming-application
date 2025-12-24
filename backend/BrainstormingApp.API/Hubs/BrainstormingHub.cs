using System.Security.Claims;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using BrainstormingApp.Application.Interfaces;
using BrainstormingApp.Core.Interfaces;

namespace BrainstormingApp.API.Hubs;

[Authorize]
public class BrainstormingHub : Hub
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISessionService _sessionService;
    private readonly ITeamService _teamService;
    private readonly IReportingService _reportingService;

    // Track online users per session
    private static readonly ConcurrentDictionary<string, HashSet<string>> SessionUsers = new();
    private static readonly ConcurrentDictionary<string, Guid> ConnectionUserMap = new();

    public BrainstormingHub(
        IUnitOfWork unitOfWork,
        ISessionService sessionService,
        ITeamService teamService,
        IReportingService reportingService)
    {
        _unitOfWork = unitOfWork;
        _sessionService = sessionService;
        _teamService = teamService;
        _reportingService = reportingService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        if (userId.HasValue)
        {
            ConnectionUserMap[Context.ConnectionId] = userId.Value;

            var httpContext = Context.GetHttpContext();
            var sessionId = httpContext?.Request.Query["sessionId"].ToString();

            if (!string.IsNullOrEmpty(sessionId) && Guid.TryParse(sessionId, out var sessionGuid))
            {
                await JoinSession(sessionId);
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (ConnectionUserMap.TryRemove(Context.ConnectionId, out var userId))
        {
            // Remove from all sessions
            foreach (var sessionKvp in SessionUsers)
            {
                if (sessionKvp.Value.Contains(Context.ConnectionId))
                {
                    sessionKvp.Value.Remove(Context.ConnectionId);
                    await Clients.Group($"session_{sessionKvp.Key}").SendAsync("MemberLeft", new
                    {
                        userId,
                        connectionId = Context.ConnectionId
                    });
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a brainstorming session
    /// </summary>
    public async Task JoinSession(string sessionId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return;

        // Verify user is a team member
        if (Guid.TryParse(sessionId, out var sessionGuid))
        {
            var session = await _unitOfWork.BrainstormingSessions.GetByIdAsync(sessionGuid);
            if (session == null) return;

            var isMember = await _teamService.IsMemberAsync(session.TeamId, userId.Value);
            if (!isMember) return;

            // Add to SignalR group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionId}");

            // Track connection
            var users = SessionUsers.GetOrAdd(sessionId, _ => new HashSet<string>());
            lock (users)
            {
                users.Add(Context.ConnectionId);
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);

            // Notify other members
            await Clients.Group($"session_{sessionId}").SendAsync("MemberJoined", new
            {
                userId = userId.Value,
                connectionId = Context.ConnectionId,
                fullName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown"
            });

            // Log action
            await _reportingService.LogActionAsync(sessionGuid, userId, "UserJoinedSession", null);

            // Send current session state to the joining user
            var sessionDetail = await _sessionService.GetSessionByIdAsync(sessionGuid);
            await Clients.Caller.SendAsync("SessionState", sessionDetail);
        }
    }

    /// <summary>
    /// Leave a brainstorming session
    /// </summary>
    public async Task LeaveSession(string sessionId)
    {
        var userId = GetCurrentUserId();

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session_{sessionId}");

        if (SessionUsers.TryGetValue(sessionId, out var users))
        {
            lock (users)
            {
                users.Remove(Context.ConnectionId);
            }
        }

        await Clients.Group($"session_{sessionId}").SendAsync("MemberLeft", new
        {
            userId,
            connectionId = Context.ConnectionId
        });

        // Log action
        if (userId.HasValue && Guid.TryParse(sessionId, out var sessionGuid))
        {
            await _reportingService.LogActionAsync(sessionGuid, userId, "UserLeftSession", null);
        }
    }

    /// <summary>
    /// Broadcast idea submission to session members
    /// </summary>
    public async Task SubmitIdea(string sessionId, object idea)
    {
        await Clients.Group($"session_{sessionId}").SendAsync("IdeaSubmitted", idea);
    }

    /// <summary>
    /// Broadcast idea update to session members
    /// </summary>
    public async Task UpdateIdea(string sessionId, object idea)
    {
        await Clients.Group($"session_{sessionId}").SendAsync("IdeaUpdated", idea);
    }

    /// <summary>
    /// Broadcast round start
    /// </summary>
    public async Task StartRound(string sessionId, int roundNumber)
    {
        await Clients.Group($"session_{sessionId}").SendAsync("RoundStarted", roundNumber);
    }

    /// <summary>
    /// Broadcast round end
    /// </summary>
    public async Task EndRound(string sessionId, int roundNumber)
    {
        await Clients.Group($"session_{sessionId}").SendAsync("RoundEnded", roundNumber);
    }

    /// <summary>
    /// Broadcast timer update (called every second during active round)
    /// </summary>
    public async Task UpdateTimer(string sessionId, int remainingSeconds)
    {
        await Clients.Group($"session_{sessionId}").SendAsync("TimerUpdate", remainingSeconds);
    }

    /// <summary>
    /// Broadcast session status change
    /// </summary>
    public async Task UpdateSessionStatus(string sessionId, string status)
    {
        await Clients.Group($"session_{sessionId}").SendAsync("SessionStatusChanged", status);
    }

    /// <summary>
    /// Send notification to session members
    /// </summary>
    public async Task SendNotification(string sessionId, string message, string type = "info")
    {
        await Clients.Group($"session_{sessionId}").SendAsync("NotificationReceived", new
        {
            message,
            type,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get online users in a session
    /// </summary>
    public async Task GetOnlineUsers(string sessionId)
    {
        var onlineUsers = new List<object>();

        if (SessionUsers.TryGetValue(sessionId, out var connectionIds))
        {
            foreach (var connectionId in connectionIds.ToList())
            {
                if (ConnectionUserMap.TryGetValue(connectionId, out var userId))
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(userId);
                    if (user != null)
                    {
                        onlineUsers.Add(new
                        {
                            userId,
                            connectionId,
                            fullName = $"{user.FirstName} {user.LastName}"
                        });
                    }
                }
            }
        }

        await Clients.Caller.SendAsync("OnlineUsers", onlineUsers);
    }

    /// <summary>
    /// Request typing indicator broadcast
    /// </summary>
    public async Task SendTypingIndicator(string sessionId, bool isTyping)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return;

        await Clients.OthersInGroup($"session_{sessionId}").SendAsync("UserTyping", new
        {
            userId = userId.Value,
            isTyping
        });
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }
}
