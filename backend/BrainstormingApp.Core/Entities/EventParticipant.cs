using BrainstormingApp.Core.Enums;

namespace BrainstormingApp.Core.Entities;

public class EventParticipant : BaseEntity
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public UserRole Role { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Event? Event { get; set; }
    public User? User { get; set; }
}
