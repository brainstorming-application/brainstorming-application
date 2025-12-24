using BrainstormingApp.Core.Enums;

namespace BrainstormingApp.Core.Entities;

public class Event : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Planned;
    public Guid CreatedById { get; set; }

    // Navigation properties
    public User? CreatedBy { get; set; }
    public ICollection<Topic> Topics { get; set; } = new List<Topic>();
    public ICollection<EventParticipant> EventParticipants { get; set; } = new List<EventParticipant>();
    public ICollection<Team> Teams { get; set; } = new List<Team>();
}
