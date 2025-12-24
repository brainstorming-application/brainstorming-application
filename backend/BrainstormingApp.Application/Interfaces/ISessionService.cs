using BrainstormingApp.Application.DTOs.Session;
using BrainstormingApp.Core.Enums;

namespace BrainstormingApp.Application.Interfaces;

public interface ISessionService
{
    Task<IEnumerable<SessionDto>> GetSessionsByTeamAsync(Guid teamId);
    Task<IEnumerable<SessionDto>> GetSessionsByTopicAsync(Guid topicId);
    Task<SessionDetailDto?> GetSessionByIdAsync(Guid sessionId);
    Task<SessionDto> CreateSessionAsync(CreateSessionDto dto, Guid userId);
    Task<SessionDto> StartSessionAsync(Guid sessionId, Guid userId);
    Task<SessionDto> PauseSessionAsync(Guid sessionId, Guid userId);
    Task<SessionDto> ResumeSessionAsync(Guid sessionId, Guid userId);
    Task<SessionDto> EndSessionAsync(Guid sessionId, Guid userId);
    Task<RoundDto> AdvanceRoundAsync(Guid sessionId, Guid userId);
    Task<RoundDto?> GetCurrentRoundAsync(Guid sessionId);
    Task<SessionStatus> GetSessionStatusAsync(Guid sessionId);
    Task<int> GetRemainingTimeAsync(Guid sessionId);
}
