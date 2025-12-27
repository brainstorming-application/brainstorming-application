using BrainstormingApp.Core.Enums;

namespace BrainstormingApp.Application.DTOs.Team;

public class TeamDetailDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? LeaderId { get; set; }
    public string? LeaderName { get; set; }
    public int MaxMembers { get; set; }
    public int CurrentMemberCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TeamMemberDetailDto> Members { get; set; } = new();
}

public class CreateTeamDto
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? LeaderId { get; set; }
    public int MaxMembers { get; set; } = 6;
}

public class UpdateTeamDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? LeaderId { get; set; }
}

public class TeamMemberDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsLeader { get; set; }
}

public class AddTeamMemberDto
{
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
}
