using BrainstormingApp.Core.Enums;

namespace BrainstormingApp.Core.Entities;

public class Topic : BaseEntity
{
    public Guid EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TopicStatus Status { get; set; } = TopicStatus.Open;

    // Navigation properties
    public Event? Event { get; set; }
    public ICollection<BrainstormingSession> BrainstormingSessions { get; set; } = new List<BrainstormingSession>();
}
