namespace BrainstormingApp.Core.Entities;

public class Team : BaseEntity
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? LeaderId { get; set; }
    public int MaxMembers { get; set; } = 6; // For 6-3-5 method

    // Navigation properties
    public Event? Event { get; set; }
    public User? Leader { get; set; }
    public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
    public ICollection<BrainstormingSession> BrainstormingSessions { get; set; } = new List<BrainstormingSession>();
}
