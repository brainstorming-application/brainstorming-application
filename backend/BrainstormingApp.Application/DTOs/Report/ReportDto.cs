namespace BrainstormingApp.Application.DTOs.Report;

public class SessionAnalyticsDto
{
    public Guid SessionId { get; set; }
    public string TopicTitle { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public int TotalIdeas { get; set; }
    public int TotalRounds { get; set; }
    public int CompletedRounds { get; set; }
    public int ParticipantCount { get; set; }
    public double AverageIdeasPerParticipant { get; set; }
    public double AverageIdeasPerRound { get; set; }
    public int AIGeneratedIdeas { get; set; }
    public int HumanGeneratedIdeas { get; set; }
    public double AIUsageRatio { get; set; }
    public TimeSpan? TotalDuration { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public List<RoundAnalyticsDto> RoundBreakdown { get; set; } = new();
    public List<ParticipantAnalyticsDto> ParticipantBreakdown { get; set; } = new();
}

public class RoundAnalyticsDto
{
    public int RoundNumber { get; set; }
    public int IdeaCount { get; set; }
    public TimeSpan? Duration { get; set; }
    public List<string> TopIdeas { get; set; } = new();
}

public class ParticipantAnalyticsDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int IdeaCount { get; set; }
    public int AIAssistedIdeas { get; set; }
}

public class SessionLogDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserFullName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
}

public class CreateSessionLogDto
{
    public Guid SessionId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
}

public class EventAnalyticsDto
{
    public Guid EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int TotalIdeas { get; set; }
    public int TotalParticipants { get; set; }
    public int TotalTeams { get; set; }
    public int TotalTopics { get; set; }
    public double AverageIdeasPerSession { get; set; }
    public List<SessionAnalyticsDto> SessionBreakdown { get; set; } = new();
}
