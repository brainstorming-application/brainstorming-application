namespace BrainstormingApp.Core.Entities;

public class SessionLog : BaseEntity
{
    public Guid SessionId { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty; // SessionStarted, RoundStarted, IdeaSubmitted, etc.
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public BrainstormingSession? Session { get; set; }
    public User? User { get; set; }
}
