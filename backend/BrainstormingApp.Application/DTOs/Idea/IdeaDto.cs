namespace BrainstormingApp.Application.DTOs.Idea;

public class IdeaDto
{
    public Guid Id { get; set; }
    public Guid RoundId { get; set; }
    public int RoundNumber { get; set; }
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int OrderInRound { get; set; }
    public bool IsAIGenerated { get; set; }
    public string? AIAnnotation { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class CreateIdeaDto
{
    public Guid SessionId { get; set; }
    public Guid? RoundId { get; set; }  // Optional - will use current round if not provided
    public string Content { get; set; } = string.Empty;
    public bool IsAIGenerated { get; set; } = false;
}

public class UpdateIdeaDto
{
    public string Content { get; set; } = string.Empty;
}

public class IdeasByRoundDto
{
    public int RoundNumber { get; set; }
    public Guid RoundId { get; set; }
    public List<IdeaDto> Ideas { get; set; } = new();
}
