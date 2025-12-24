using BrainstormingApp.Core.Enums;

namespace BrainstormingApp.Core.Entities;

public class BrainstormingSession : BaseEntity
{
    public Guid TeamId { get; set; }
    public Guid TopicId { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.NotStarted;
    public int CurrentRound { get; set; } = 0;
    public int TotalRounds { get; set; } = 5; // For 6-3-5 method
    public int RoundDurationMinutes { get; set; } = 5;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    // Navigation properties
    public Team? Team { get; set; }
    public Topic? Topic { get; set; }
    public ICollection<Round> Rounds { get; set; } = new List<Round>();
    public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
    public ICollection<SessionLog> SessionLogs { get; set; } = new List<SessionLog>();
    public ICollection<ChatGPTInteraction> ChatGPTInteractions { get; set; } = new List<ChatGPTInteraction>();
}
