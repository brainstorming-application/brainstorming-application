using BrainstormingApp.Core.Enums;

namespace BrainstormingApp.Application.DTOs.Session;

public class SessionDto
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public Guid TopicId { get; set; }
    public string TopicTitle { get; set; } = string.Empty;
    public SessionStatus Status { get; set; }
    public int CurrentRound { get; set; }
    public int TotalRounds { get; set; }
    public int RoundDurationMinutes { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalIdeas { get; set; }
    public int ParticipantCount { get; set; }
}

public class CreateSessionDto
{
    public Guid TeamId { get; set; }
    public Guid TopicId { get; set; }
    public int TotalRounds { get; set; } = 5;
    public int RoundDurationMinutes { get; set; } = 5;
}

public class SessionDetailDto : SessionDto
{
    public List<RoundDto> Rounds { get; set; } = new();
    public List<ParticipantDto> Participants { get; set; } = new();
}

public class RoundDto
{
    public Guid Id { get; set; }
    public int RoundNumber { get; set; }
    public SessionStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int? DurationSeconds { get; set; }
    public int IdeaCount { get; set; }
}

public class ParticipantDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
}
