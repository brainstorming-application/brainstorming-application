using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BrainstormingApp.Application.DTOs.ChatGPT;
using BrainstormingApp.Application.Interfaces;
using BrainstormingApp.Core.Entities;
using BrainstormingApp.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BrainstormingApp.Application.Services;

public class ChatGPTService : IChatGPTService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _apiUrl;

    // Rate limiting
    private const int MaxRequestsPerSession = 10;
    private const int ApiTimeoutSeconds = 30;

    public ChatGPTService(IUnitOfWork unitOfWork, IConfiguration configuration, HttpClient httpClient)
    {
        _unitOfWork = unitOfWork;
        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"] ?? "";
        _apiUrl = configuration["OpenAI:ApiUrl"] ?? "https://api.openai.com/v1/chat/completions";
    }

    public async Task<GenerateIdeasResponseDto> GenerateIdeasAsync(GenerateIdeasRequestDto dto, Guid userId)
    {
        // Validate rate limit
        if (!await ValidateRateLimitAsync(userId, dto.SessionId))
        {
            throw new InvalidOperationException($"Rate limit exceeded. Maximum {MaxRequestsPerSession} AI requests per session.");
        }

        var existingIdeasContext = dto.ExistingIdeas?.Any() == true
            ? $"\n\nExisting ideas to build upon:\n{string.Join("\n", dto.ExistingIdeas.Select((idea, i) => $"{i + 1}. {idea}"))}"
            : "";

        var prompt = $@"You are a creative brainstorming assistant helping with the 6-3-5 brainwriting method.

Topic: {dto.TopicDescription}
{existingIdeasContext}

Generate exactly {dto.Count} unique, creative, and actionable ideas for this topic.
Each idea should be:
- Concise (max 100 words)
- Practical and implementable
- Different from existing ideas if provided
- Building on or complementing existing ideas when relevant

Format your response as a JSON array of strings, like this:
[""idea 1"", ""idea 2"", ""idea 3""]

Only respond with the JSON array, no additional text.";

        var response = await CallOpenAIAsync(prompt);
        var ideas = ParseIdeasFromResponse(response.Content);

        // Log interaction
        await LogInteractionAsync(dto.SessionId, userId, prompt, response.Content, "IdeaGeneration", response.TokensUsed);

        return new GenerateIdeasResponseDto
        {
            GeneratedIdeas = ideas,
            TokensUsed = response.TokensUsed
        };
    }

    public async Task<GenerateSummaryResponseDto> GenerateSummaryAsync(GenerateSummaryRequestDto dto, Guid userId)
    {
        // Get all ideas from session
        var ideas = await _unitOfWork.Ideas.FindAsync(i => i.SessionId == dto.SessionId);
        var ideaContents = ideas.Select(i => i.Content).ToList();

        if (!ideaContents.Any())
        {
            return new GenerateSummaryResponseDto
            {
                Summary = "No ideas to summarize.",
                KeyThemes = new List<string>(),
                TopIdeas = new List<string>(),
                TokensUsed = 0
            };
        }

        var session = await _unitOfWork.BrainstormingSessions.GetByIdAsync(dto.SessionId);
        var topic = session != null ? await _unitOfWork.Topics.GetByIdAsync(session.TopicId) : null;

        var prompt = $@"Analyze and summarize the following brainstorming session ideas.

Topic: {topic?.Title ?? "Unknown"}
Topic Description: {topic?.Description ?? "No description"}

Ideas ({ideaContents.Count} total):
{string.Join("\n", ideaContents.Select((idea, i) => $"{i + 1}. {idea}"))}

Provide:
1. A concise summary (2-3 sentences) of the brainstorming session outcomes
2. 3-5 key themes that emerged from the ideas
3. The top 5 most promising or innovative ideas

Format your response as JSON:
{{
  ""summary"": ""your summary here"",
  ""keyThemes"": [""theme1"", ""theme2"", ...],
  ""topIdeas"": [""idea1"", ""idea2"", ...]
}}

Only respond with the JSON, no additional text.";

        var response = await CallOpenAIAsync(prompt);

        // Log interaction
        await LogInteractionAsync(dto.SessionId, userId, prompt, response.Content, "Summary", response.TokensUsed);

        return ParseSummaryFromResponse(response.Content, response.TokensUsed);
    }

    public async Task<GenerateAnnotationResponseDto> GenerateAnnotationAsync(GenerateAnnotationRequestDto dto, Guid userId)
    {
        var idea = await _unitOfWork.Ideas.GetByIdAsync(dto.IdeaId);
        if (idea == null)
        {
            throw new InvalidOperationException("Idea not found");
        }

        var prompt = $@"Analyze this brainstorming idea and provide helpful feedback:

Idea: {dto.IdeaContent}

Provide:
1. A brief annotation explaining the idea's potential and considerations
2. 2-3 specific suggestions for improving or expanding on this idea

Format your response as JSON:
{{
  ""annotation"": ""your annotation here"",
  ""suggestions"": [""suggestion1"", ""suggestion2"", ...]
}}

Only respond with the JSON, no additional text.";

        var response = await CallOpenAIAsync(prompt);

        // Log interaction
        await LogInteractionAsync(idea.SessionId, userId, prompt, response.Content, "Annotation", response.TokensUsed);

        // Update idea with annotation
        var result = ParseAnnotationFromResponse(response.Content, response.TokensUsed);
        idea.AIAnnotation = result.Annotation;
        idea.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Ideas.UpdateAsync(idea);
        await _unitOfWork.SaveChangesAsync();

        return result;
    }

    public async Task<IEnumerable<ChatGPTInteractionDto>> GetInteractionsBySessionAsync(Guid sessionId)
    {
        var interactions = await _unitOfWork.ChatGPTInteractions.FindAsync(i => i.SessionId == sessionId);
        var result = new List<ChatGPTInteractionDto>();

        foreach (var interaction in interactions.OrderByDescending(i => i.CreatedAt))
        {
            var user = await _unitOfWork.Users.GetByIdAsync(interaction.UserId);
            result.Add(new ChatGPTInteractionDto
            {
                Id = interaction.Id,
                SessionId = interaction.SessionId,
                UserId = interaction.UserId,
                UserFullName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                Prompt = interaction.Prompt,
                Response = interaction.Response,
                InteractionType = interaction.InteractionType ?? "Unknown",
                TokensUsed = interaction.TokensUsed,
                CreatedAt = interaction.CreatedAt
            });
        }

        return result;
    }

    public async Task<IEnumerable<ChatGPTInteractionDto>> GetInteractionsByUserAsync(Guid userId)
    {
        var interactions = await _unitOfWork.ChatGPTInteractions.FindAsync(i => i.UserId == userId);
        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        return interactions.OrderByDescending(i => i.CreatedAt).Select(interaction => new ChatGPTInteractionDto
        {
            Id = interaction.Id,
            SessionId = interaction.SessionId,
            UserId = interaction.UserId,
            UserFullName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
            Prompt = interaction.Prompt,
            Response = interaction.Response,
            InteractionType = interaction.InteractionType ?? "Unknown",
            TokensUsed = interaction.TokensUsed,
            CreatedAt = interaction.CreatedAt
        });
    }

    public async Task<bool> ValidateRateLimitAsync(Guid userId, Guid sessionId)
    {
        var count = await GetUserRequestCountAsync(userId, sessionId);
        return count < MaxRequestsPerSession;
    }

    public async Task<int> GetUserRequestCountAsync(Guid userId, Guid sessionId)
    {
        return await _unitOfWork.ChatGPTInteractions.CountAsync(
            i => i.UserId == userId && i.SessionId == sessionId);
    }

    private async Task<(string Content, int TokensUsed)> CallOpenAIAsync(string prompt)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            // Return mock response for development without API key
            return (GetMockResponse(prompt), 0);
        }

        try
        {
            var request = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                max_tokens = 1000,
                temperature = 0.7
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.PostAsJsonAsync(_apiUrl, request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
            var content = result?.Choices?.FirstOrDefault()?.Message?.Content ?? "";
            var tokensUsed = result?.Usage?.TotalTokens ?? 0;

            return (content, tokensUsed);
        }
        catch (Exception ex)
        {
            // Log error and return mock response
            Console.WriteLine($"OpenAI API error: {ex.Message}");
            return (GetMockResponse(prompt), 0);
        }
    }

    private string GetMockResponse(string prompt)
    {
        if (prompt.Contains("Generate exactly"))
        {
            return @"[""Innovative idea for better collaboration"", ""Creative solution using modern technology"", ""Sustainable approach to the problem""]";
        }
        if (prompt.Contains("Analyze and summarize"))
        {
            return @"{""summary"": ""The brainstorming session generated diverse ideas focusing on innovation and practical solutions."", ""keyThemes"": [""Innovation"", ""Collaboration"", ""Sustainability""], ""topIdeas"": [""Digital transformation initiative"", ""Team-based problem solving""]}";
        }
        return @"{""annotation"": ""This is a promising idea with good potential for implementation."", ""suggestions"": [""Consider scalability"", ""Add measurable outcomes""]}";
    }

    private List<string> ParseIdeasFromResponse(string response)
    {
        try
        {
            var ideas = JsonSerializer.Deserialize<List<string>>(response);
            return ideas ?? new List<string>();
        }
        catch
        {
            // Try to extract ideas from non-JSON response
            return new List<string> { response };
        }
    }

    private GenerateSummaryResponseDto ParseSummaryFromResponse(string response, int tokensUsed)
    {
        try
        {
            var result = JsonSerializer.Deserialize<JsonElement>(response);
            return new GenerateSummaryResponseDto
            {
                Summary = result.GetProperty("summary").GetString() ?? "",
                KeyThemes = result.GetProperty("keyThemes").EnumerateArray()
                    .Select(e => e.GetString() ?? "").ToList(),
                TopIdeas = result.GetProperty("topIdeas").EnumerateArray()
                    .Select(e => e.GetString() ?? "").ToList(),
                TokensUsed = tokensUsed
            };
        }
        catch
        {
            return new GenerateSummaryResponseDto
            {
                Summary = response,
                KeyThemes = new List<string>(),
                TopIdeas = new List<string>(),
                TokensUsed = tokensUsed
            };
        }
    }

    private GenerateAnnotationResponseDto ParseAnnotationFromResponse(string response, int tokensUsed)
    {
        try
        {
            var result = JsonSerializer.Deserialize<JsonElement>(response);
            return new GenerateAnnotationResponseDto
            {
                Annotation = result.GetProperty("annotation").GetString() ?? "",
                Suggestions = result.GetProperty("suggestions").EnumerateArray()
                    .Select(e => e.GetString() ?? "").ToList(),
                TokensUsed = tokensUsed
            };
        }
        catch
        {
            return new GenerateAnnotationResponseDto
            {
                Annotation = response,
                Suggestions = new List<string>(),
                TokensUsed = tokensUsed
            };
        }
    }

    private async Task LogInteractionAsync(Guid? sessionId, Guid userId, string prompt, string response, string interactionType, int tokensUsed)
    {
        var interaction = new ChatGPTInteraction
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            UserId = userId,
            Prompt = prompt,
            Response = response,
            InteractionType = interactionType,
            TokensUsed = tokensUsed,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ChatGPTInteractions.AddAsync(interaction);
        await _unitOfWork.SaveChangesAsync();
    }

    // OpenAI Response Models
    private class OpenAIResponse
    {
        public List<Choice>? Choices { get; set; }
        public Usage? Usage { get; set; }
    }

    private class Choice
    {
        public Message? Message { get; set; }
    }

    private class Message
    {
        public string? Content { get; set; }
    }

    private class Usage
    {
        public int TotalTokens { get; set; }
    }
}
