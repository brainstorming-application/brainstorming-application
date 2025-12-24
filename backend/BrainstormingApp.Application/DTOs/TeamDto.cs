namespace BrainstormingApp.Application.DTOs;

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
    public int MaxMembers { get; set; }
}

public class TeamDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? LeaderId { get; set; }
    public int MaxMembers { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
