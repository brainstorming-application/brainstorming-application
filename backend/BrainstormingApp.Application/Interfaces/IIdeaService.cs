using BrainstormingApp.Application.DTOs.Idea;

namespace BrainstormingApp.Application.Interfaces;

public interface IIdeaService
{
    Task<IdeaDto> SubmitIdeaAsync(CreateIdeaDto dto, Guid userId);
    Task<IdeaDto> UpdateIdeaAsync(Guid ideaId, UpdateIdeaDto dto, Guid userId);
    Task<IEnumerable<IdeaDto>> GetIdeasByRoundAsync(Guid roundId);
    Task<IEnumerable<IdeaDto>> GetIdeasBySessionAsync(Guid sessionId);
    Task<IEnumerable<IdeasByRoundDto>> GetIdeasGroupedByRoundAsync(Guid sessionId);
    Task<IEnumerable<IdeaDto>> GetUserIdeasInRoundAsync(Guid roundId, Guid userId);
    Task<int> GetUserIdeaCountInRoundAsync(Guid roundId, Guid userId);
    Task<bool> ValidateIdeaLimitAsync(Guid roundId, Guid userId);
    Task<bool> CanUserSubmitAsync(Guid sessionId, Guid userId);
    Task DeleteIdeaAsync(Guid ideaId, Guid userId);
}
