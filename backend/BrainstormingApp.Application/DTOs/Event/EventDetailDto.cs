using BrainstormingApp.Core.Enums;

namespace BrainstormingApp.Application.DTOs.Event;

public class EventDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public EventStatus Status { get; set; }
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int TopicCount { get; set; }
    public int TeamCount { get; set; }
    public int ParticipantCount { get; set; }
}

public class CreateEventDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class UpdateEventDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public EventStatus? Status { get; set; }
}

public class EventParticipantDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class AddParticipantDto
{
    public Guid UserId { get; set; }
    public UserRole Role { get; set; } = UserRole.TeamMember;
}
