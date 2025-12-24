namespace BrainstormingApp.Application.DTOs.ChatGPT;

public class ChatGPTInteractionDto
{
    public Guid Id { get; set; }
    public Guid? SessionId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string InteractionType { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GenerateIdeasRequestDto
{
    public Guid SessionId { get; set; }
    public string TopicDescription { get; set; } = string.Empty;
    public int Count { get; set; } = 3;
    public List<string>? ExistingIdeas { get; set; }
}

public class GenerateIdeasResponseDto
{
    public List<string> GeneratedIdeas { get; set; } = new();
    public int TokensUsed { get; set; }
}

public class GenerateSummaryRequestDto
{
    public Guid SessionId { get; set; }
}

public class GenerateSummaryResponseDto
{
    public string Summary { get; set; } = string.Empty;
    public List<string> KeyThemes { get; set; } = new();
    public List<string> TopIdeas { get; set; } = new();
    public int TokensUsed { get; set; }
}

public class GenerateAnnotationRequestDto
{
    public Guid IdeaId { get; set; }
    public string IdeaContent { get; set; } = string.Empty;
}

public class GenerateAnnotationResponseDto
{
    public string Annotation { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
    public int TokensUsed { get; set; }
}
