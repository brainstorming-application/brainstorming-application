namespace BrainstormingApp.Core.Entities;

public class ChatGPTInteraction : BaseEntity
{
    public Guid? SessionId { get; set; }
    public Guid UserId { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string? InteractionType { get; set; } // IdeaGeneration, Summarization, Suggestion
    public int TokensUsed { get; set; } = 0;

    // Navigation properties
    public BrainstormingSession? Session { get; set; }
    public User? User { get; set; }
}
