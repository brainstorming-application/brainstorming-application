namespace BrainstormingApp.Core.Entities;

public class Idea : BaseEntity
{
    public Guid RoundId { get; set; }
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int OrderInRound { get; set; } // 1, 2, or 3 for 6-3-5 method
    public bool IsAIGenerated { get; set; } = false;
    public string? AIAnnotation { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Round? Round { get; set; }
    public BrainstormingSession? Session { get; set; }
    public User? User { get; set; }
}
