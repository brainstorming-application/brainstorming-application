using BrainstormingApp.Core.Enums;

namespace BrainstormingApp.Core.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<EventParticipant> EventParticipants { get; set; } = new List<EventParticipant>();
    public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();
    public ICollection<Team> LeadingTeams { get; set; } = new List<Team>();
    public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
    public ICollection<ChatGPTInteraction> ChatGPTInteractions { get; set; } = new List<ChatGPTInteraction>();
}
