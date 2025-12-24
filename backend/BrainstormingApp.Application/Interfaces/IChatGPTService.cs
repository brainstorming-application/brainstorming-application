using BrainstormingApp.Application.DTOs.ChatGPT;

namespace BrainstormingApp.Application.Interfaces;

public interface IChatGPTService
{
    Task<GenerateIdeasResponseDto> GenerateIdeasAsync(GenerateIdeasRequestDto dto, Guid userId);
    Task<GenerateSummaryResponseDto> GenerateSummaryAsync(GenerateSummaryRequestDto dto, Guid userId);
    Task<GenerateAnnotationResponseDto> GenerateAnnotationAsync(GenerateAnnotationRequestDto dto, Guid userId);
    Task<IEnumerable<ChatGPTInteractionDto>> GetInteractionsBySessionAsync(Guid sessionId);
    Task<IEnumerable<ChatGPTInteractionDto>> GetInteractionsByUserAsync(Guid userId);
    Task<bool> ValidateRateLimitAsync(Guid userId, Guid sessionId);
    Task<int> GetUserRequestCountAsync(Guid userId, Guid sessionId);
}
