using BrainstormingApp.Core.Enums;

namespace BrainstormingApp.Core.Entities;

public class Round : BaseEntity
{
    public Guid SessionId { get; set; }
    public int RoundNumber { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.NotStarted;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int? DurationSeconds { get; set; }

    // Navigation properties
    public BrainstormingSession? Session { get; set; }
    public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
}
