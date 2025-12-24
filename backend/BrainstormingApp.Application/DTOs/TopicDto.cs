using BrainstormingApp.Core.Enums;

namespace BrainstormingApp.Application.DTOs;

public class CreateTopicDto
{
    public Guid EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateTopicDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TopicStatus Status { get; set; }
}

public class TopicDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TopicStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
