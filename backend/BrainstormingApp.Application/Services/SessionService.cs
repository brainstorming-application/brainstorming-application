using BrainstormingApp.Application.DTOs.Session;
using BrainstormingApp.Application.Interfaces;
using BrainstormingApp.Core.Entities;
using BrainstormingApp.Core.Enums;
using BrainstormingApp.Core.Interfaces;

namespace BrainstormingApp.Application.Services;

public class SessionService : ISessionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITeamService _teamService;

    // 6-3-5 Method constants
    private const int DefaultTotalRounds = 5;
    private const int DefaultRoundDurationMinutes = 5;
    private const int MinTeamSize = 3;
    private const int MaxTeamSize = 6;

    public SessionService(IUnitOfWork unitOfWork, ITeamService teamService)
    {
        _unitOfWork = unitOfWork;
        _teamService = teamService;
    }

    public async Task<IEnumerable<SessionDto>> GetSessionsByTeamAsync(Guid teamId)
    {
        var sessions = await _unitOfWork.BrainstormingSessions.FindAsync(s => s.TeamId == teamId);
        var result = new List<SessionDto>();

        foreach (var session in sessions)
        {
            result.Add(await MapToDto(session));
        }

        return result.OrderByDescending(s => s.CreatedAt);
    }

    public async Task<IEnumerable<SessionDto>> GetSessionsByTopicAsync(Guid topicId)
    {
        var sessions = await _unitOfWork.BrainstormingSessions.FindAsync(s => s.TopicId == topicId);
        var result = new List<SessionDto>();

        foreach (var session in sessions)
        {
            result.Add(await MapToDto(session));
        }

        return result.OrderByDescending(s => s.CreatedAt);
    }

    public async Task<IEnumerable<SessionDto>> GetMySessionsAsync(Guid userId)
    {
        // Get all team memberships for this user
        var memberships = await _unitOfWork.TeamMembers.FindAsync(m => m.UserId == userId);
        var teamIds = memberships.Select(m => m.TeamId).ToList();

        var result = new List<SessionDto>();
        foreach (var teamId in teamIds)
        {
            var sessions = await _unitOfWork.BrainstormingSessions.FindAsync(s => s.TeamId == teamId);
            foreach (var session in sessions)
            {
                result.Add(await MapToDto(session));
            }
        }

        return result.OrderByDescending(s => s.CreatedAt);
    }

    public async Task<SessionDetailDto?> GetSessionByIdAsync(Guid sessionId)
    {
        var session = await _unitOfWork.BrainstormingSessions.GetByIdAsync(sessionId);
        if (session == null) return null;

        return await MapToDetailDto(session);
    }

    public async Task<SessionDto> CreateSessionAsync(CreateSessionDto dto, Guid userId)
    {
        // Validate team exists and has valid size
        var team = await _unitOfWork.Teams.GetByIdAsync(dto.TeamId);
        if (team == null)
        {
            throw new InvalidOperationException("Team not found");
        }

        var memberCount = await _teamService.GetMemberCountAsync(dto.TeamId);
        if (memberCount < MinTeamSize)
        {
            throw new InvalidOperationException($"Team must have at least {MinTeamSize} members for a brainstorming session");
        }

        if (memberCount > MaxTeamSize)
        {
            throw new InvalidOperationException($"Team cannot have more than {MaxTeamSize} members (6-3-5 method constraint)");
        }

        // Validate topic exists and is open
        var topic = await _unitOfWork.Topics.GetByIdAsync(dto.TopicId);
        if (topic == null)
        {
            throw new InvalidOperationException("Topic not found");
        }

        if (topic.Status != TopicStatus.Open)
        {
            throw new InvalidOperationException("Topic is not open for brainstorming");
        }

        // Verify user is team leader or event manager
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user?.Role != UserRole.EventManager && team.LeaderId != userId)
        {
            throw new UnauthorizedAccessException("Only EventManagers or team leaders can create sessions");
        }

        // Check for existing active session
        var existingActive = await _unitOfWork.BrainstormingSessions.FirstOrDefaultAsync(
            s => s.TeamId == dto.TeamId && s.Status == SessionStatus.InProgress);
        if (existingActive != null)
        {
            throw new InvalidOperationException("Team already has an active session");
        }

        var session = new BrainstormingSession
        {
            Id = Guid.NewGuid(),
            TeamId = dto.TeamId,
            TopicId = dto.TopicId,
            Status = SessionStatus.NotStarted,
            CurrentRound = 0,
            TotalRounds = dto.TotalRounds > 0 ? dto.TotalRounds : DefaultTotalRounds,
            RoundDurationMinutes = dto.RoundDurationMinutes > 0 ? dto.RoundDurationMinutes : DefaultRoundDurationMinutes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.BrainstormingSessions.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDto(session);
    }

    public async Task<SessionDto> StartSessionAsync(Guid sessionId, Guid userId)
    {
        var session = await _unitOfWork.BrainstormingSessions.GetByIdAsync(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Session not found");
        }

        if (session.Status != SessionStatus.NotStarted && session.Status != SessionStatus.Paused)
        {
            throw new InvalidOperationException($"Cannot start session with status {session.Status}");
        }

        // Verify user has permission
        var team = await _unitOfWork.Teams.GetByIdAsync(session.TeamId);
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user?.Role != UserRole.EventManager && team?.LeaderId != userId)
        {
            throw new UnauthorizedAccessException("Only EventManagers or team leaders can start sessions");
        }

        session.Status = SessionStatus.InProgress;
        session.StartedAt = session.StartedAt ?? DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;

        // Create first round if not exists
        if (session.CurrentRound == 0)
        {
            session.CurrentRound = 1;
            var firstRound = new Round
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                RoundNumber = 1,
                Status = SessionStatus.InProgress,
                StartedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Rounds.AddAsync(firstRound);
        }
        else
        {
            // Resume current round
            var currentRound = await _unitOfWork.Rounds.FirstOrDefaultAsync(
                r => r.SessionId == sessionId && r.RoundNumber == session.CurrentRound);
            if (currentRound != null)
            {
                currentRound.Status = SessionStatus.InProgress;
                currentRound.StartedAt = currentRound.StartedAt ?? DateTime.UtcNow;
                await _unitOfWork.Rounds.UpdateAsync(currentRound);
            }
        }

        await _unitOfWork.BrainstormingSessions.UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDto(session);
    }

    public async Task<SessionDto> PauseSessionAsync(Guid sessionId, Guid userId)
    {
        var session = await _unitOfWork.BrainstormingSessions.GetByIdAsync(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Session not found");
        }

        if (session.Status != SessionStatus.InProgress)
        {
            throw new InvalidOperationException("Can only pause an active session");
        }

        session.Status = SessionStatus.Paused;
        session.UpdatedAt = DateTime.UtcNow;

        // Pause current round
        var currentRound = await _unitOfWork.Rounds.FirstOrDefaultAsync(
            r => r.SessionId == sessionId && r.RoundNumber == session.CurrentRound);
        if (currentRound != null)
        {
            currentRound.Status = SessionStatus.Paused;
            await _unitOfWork.Rounds.UpdateAsync(currentRound);
        }

        await _unitOfWork.BrainstormingSessions.UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDto(session);
    }

    public async Task<SessionDto> ResumeSessionAsync(Guid sessionId, Guid userId)
    {
        return await StartSessionAsync(sessionId, userId);
    }

    public async Task<SessionDto> EndSessionAsync(Guid sessionId, Guid userId)
    {
        var session = await _unitOfWork.BrainstormingSessions.GetByIdAsync(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Session not found");
        }

        if (session.Status == SessionStatus.Completed)
        {
            throw new InvalidOperationException("Session is already completed");
        }

        session.Status = SessionStatus.Completed;
        session.EndedAt = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;

        // End current round if active
        var currentRound = await _unitOfWork.Rounds.FirstOrDefaultAsync(
            r => r.SessionId == sessionId && r.RoundNumber == session.CurrentRound);
        if (currentRound != null && currentRound.Status != SessionStatus.Completed)
        {
            currentRound.Status = SessionStatus.Completed;
            currentRound.EndedAt = DateTime.UtcNow;
            if (currentRound.StartedAt.HasValue)
            {
                currentRound.DurationSeconds = (int)(DateTime.UtcNow - currentRound.StartedAt.Value).TotalSeconds;
            }
            await _unitOfWork.Rounds.UpdateAsync(currentRound);
        }

        await _unitOfWork.BrainstormingSessions.UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDto(session);
    }

    public async Task<RoundDto> AdvanceRoundAsync(Guid sessionId, Guid userId)
    {
        var session = await _unitOfWork.BrainstormingSessions.GetByIdAsync(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Session not found");
        }

        if (session.Status != SessionStatus.InProgress)
        {
            throw new InvalidOperationException("Session must be in progress to advance rounds");
        }

        // End current round
        var currentRound = await _unitOfWork.Rounds.FirstOrDefaultAsync(
            r => r.SessionId == sessionId && r.RoundNumber == session.CurrentRound);
        if (currentRound != null)
        {
            currentRound.Status = SessionStatus.Completed;
            currentRound.EndedAt = DateTime.UtcNow;
            if (currentRound.StartedAt.HasValue)
            {
                currentRound.DurationSeconds = (int)(DateTime.UtcNow - currentRound.StartedAt.Value).TotalSeconds;
            }
            await _unitOfWork.Rounds.UpdateAsync(currentRound);
        }

        // Check if session should end
        if (session.CurrentRound >= session.TotalRounds)
        {
            await EndSessionAsync(sessionId, userId);
            return await MapRoundToDto(currentRound!);
        }

        // Create next round
        session.CurrentRound++;
        var newRound = new Round
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            RoundNumber = session.CurrentRound,
            Status = SessionStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Rounds.AddAsync(newRound);
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.BrainstormingSessions.UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return await MapRoundToDto(newRound);
    }

    public async Task<RoundDto?> GetCurrentRoundAsync(Guid sessionId)
    {
        var session = await _unitOfWork.BrainstormingSessions.GetByIdAsync(sessionId);
        if (session == null || session.CurrentRound == 0)
        {
            return null;
        }

        var round = await _unitOfWork.Rounds.FirstOrDefaultAsync(
            r => r.SessionId == sessionId && r.RoundNumber == session.CurrentRound);

        return round != null ? await MapRoundToDto(round) : null;
    }

    public async Task<SessionStatus> GetSessionStatusAsync(Guid sessionId)
    {
        var session = await _unitOfWork.BrainstormingSessions.GetByIdAsync(sessionId);
        return session?.Status ?? throw new InvalidOperationException("Session not found");
    }

    public async Task<int> GetRemainingTimeAsync(Guid sessionId)
    {
        var session = await _unitOfWork.BrainstormingSessions.GetByIdAsync(sessionId);
        if (session == null || session.Status != SessionStatus.InProgress)
        {
            return 0;
        }

        var currentRound = await _unitOfWork.Rounds.FirstOrDefaultAsync(
            r => r.SessionId == sessionId && r.RoundNumber == session.CurrentRound);

        if (currentRound?.StartedAt == null)
        {
            return session.RoundDurationMinutes * 60;
        }

        var elapsed = (int)(DateTime.UtcNow - currentRound.StartedAt.Value).TotalSeconds;
        var totalSeconds = session.RoundDurationMinutes * 60;
        var remaining = totalSeconds - elapsed;

        return Math.Max(0, remaining);
    }

    private async Task<SessionDto> MapToDto(BrainstormingSession session)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(session.TeamId);
        var topic = await _unitOfWork.Topics.GetByIdAsync(session.TopicId);
        var ideaCount = await _unitOfWork.Ideas.CountAsync(i => i.SessionId == session.Id);
        var memberCount = await _teamService.GetMemberCountAsync(session.TeamId);

        return new SessionDto
        {
            Id = session.Id,
            TeamId = session.TeamId,
            TeamName = team?.Name ?? "Unknown",
            TopicId = session.TopicId,
            TopicTitle = topic?.Title ?? "Unknown",
            Status = session.Status,
            CurrentRound = session.CurrentRound,
            TotalRounds = session.TotalRounds,
            RoundDurationMinutes = session.RoundDurationMinutes,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            CreatedAt = session.CreatedAt,
            TotalIdeas = ideaCount,
            ParticipantCount = memberCount
        };
    }

    private async Task<SessionDetailDto> MapToDetailDto(BrainstormingSession session)
    {
        var dto = await MapToDto(session);
        var rounds = await _unitOfWork.Rounds.FindAsync(r => r.SessionId == session.Id);
        var members = await _teamService.GetTeamMembersAsync(session.TeamId);

        var detailDto = new SessionDetailDto
        {
            Id = dto.Id,
            TeamId = dto.TeamId,
            TeamName = dto.TeamName,
            TopicId = dto.TopicId,
            TopicTitle = dto.TopicTitle,
            Status = dto.Status,
            CurrentRound = dto.CurrentRound,
            TotalRounds = dto.TotalRounds,
            RoundDurationMinutes = dto.RoundDurationMinutes,
            StartedAt = dto.StartedAt,
            EndedAt = dto.EndedAt,
            CreatedAt = dto.CreatedAt,
            TotalIdeas = dto.TotalIdeas,
            ParticipantCount = dto.ParticipantCount,
            Rounds = new List<RoundDto>(),
            Participants = members.Select(m => new ParticipantDto
            {
                UserId = m.UserId,
                FullName = m.FullName,
                Email = m.Email,
                IsOnline = false // Will be updated by SignalR
            }).ToList()
        };

        foreach (var round in rounds.OrderBy(r => r.RoundNumber))
        {
            detailDto.Rounds.Add(await MapRoundToDto(round));
        }

        return detailDto;
    }

    private async Task<RoundDto> MapRoundToDto(Round round)
    {
        var ideaCount = await _unitOfWork.Ideas.CountAsync(i => i.RoundId == round.Id);

        return new RoundDto
        {
            Id = round.Id,
            RoundNumber = round.RoundNumber,
            Status = round.Status,
            StartedAt = round.StartedAt,
            EndedAt = round.EndedAt,
            DurationSeconds = round.DurationSeconds,
            IdeaCount = ideaCount
        };
    }
}
