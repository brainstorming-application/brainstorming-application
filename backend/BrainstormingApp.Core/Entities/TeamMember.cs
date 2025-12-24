namespace BrainstormingApp.Core.Entities;

public class TeamMember : BaseEntity
{
    public Guid TeamId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Team? Team { get; set; }
    public User? User { get; set; }
}
