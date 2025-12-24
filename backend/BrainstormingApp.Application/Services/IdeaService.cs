using BrainstormingApp.Application.DTOs.Idea;
using BrainstormingApp.Application.Interfaces;
using BrainstormingApp.Core.Entities;
using BrainstormingApp.Core.Enums;
using BrainstormingApp.Core.Interfaces;

namespace BrainstormingApp.Application.Services;

public class IdeaService : IIdeaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITeamService _teamService;

    // 6-3-5 Method: Each participant submits exactly 3 ideas per round
    private const int MaxIdeasPerRound = 3;
    private const int MaxContentLength = 500;

    public IdeaService(IUnitOfWork unitOfWork, ITeamService teamService)
    {
        _unitOfWork = unitOfWork;
        _teamService = teamService;
    }

    public async Task<IdeaDto> SubmitIdeaAsync(CreateIdeaDto dto, Guid userId)
    {
        // Validate session exists and is active
        var session = await _unitOfWork.BrainstormingSessions.GetByIdAsync(dto.SessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Session not found");
        }

        if (session.Status != SessionStatus.InProgress)
        {
            throw new InvalidOperationException("Can only submit ideas during active sessions");
        }

        // Validate round exists and is active
        var round = await _unitOfWork.Rounds.GetByIdAsync(dto.RoundId);
        if (round == null)
        {
            throw new InvalidOperationException("Round not found");
        }

        if (round.SessionId != dto.SessionId)
        {
            throw new InvalidOperationException("Round does not belong to the specified session");
        }

        if (round.Status != SessionStatus.InProgress)
        {
            throw new InvalidOperationException("Can only submit ideas during active rounds");
        }

        // Verify user is a team member
        var isMember = await _teamService.IsMemberAsync(session.TeamId, userId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("Only team members can submit ideas");
        }

        // Validate content
        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            throw new InvalidOperationException("Idea content cannot be empty");
        }

        if (dto.Content.Length > MaxContentLength)
        {
            throw new InvalidOperationException($"Idea content cannot exceed {MaxContentLength} characters");
        }

        // Check idea limit (3 ideas per user per round)
        var userIdeaCount = await GetUserIdeaCountInRoundAsync(dto.RoundId, userId);
        if (userIdeaCount >= MaxIdeasPerRound)
        {
            throw new InvalidOperationException($"You have already submitted {MaxIdeasPerRound} ideas this round");
        }

        var idea = new Idea
        {
            Id = Guid.NewGuid(),
            RoundId = dto.RoundId,
            SessionId = dto.SessionId,
            UserId = userId,
            Content = dto.Content.Trim(),
            OrderInRound = userIdeaCount + 1, // 1, 2, or 3
            IsAIGenerated = false,
            SubmittedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Ideas.AddAsync(idea);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDto(idea);
    }

    public async Task<IdeaDto> UpdateIdeaAsync(Guid ideaId, UpdateIdeaDto dto, Guid userId)
    {
        var idea = await _unitOfWork.Ideas.GetByIdAsync(ideaId);
        if (idea == null)
        {
            throw new InvalidOperationException("Idea not found");
        }

        // Verify ownership
        if (idea.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only edit your own ideas");
        }

        // Verify round is still active
        var round = await _unitOfWork.Rounds.GetByIdAsync(idea.RoundId);
        if (round?.Status != SessionStatus.InProgress)
        {
            throw new InvalidOperationException("Cannot edit ideas after round has ended");
        }

        // Validate content
        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            throw new InvalidOperationException("Idea content cannot be empty");
        }

        if (dto.Content.Length > MaxContentLength)
        {
            throw new InvalidOperationException($"Idea content cannot exceed {MaxContentLength} characters");
        }

        idea.Content = dto.Content.Trim();
        idea.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Ideas.UpdateAsync(idea);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDto(idea);
    }

    public async Task<IEnumerable<IdeaDto>> GetIdeasByRoundAsync(Guid roundId)
    {
        var ideas = await _unitOfWork.Ideas.FindAsync(i => i.RoundId == roundId);
        var result = new List<IdeaDto>();

        foreach (var idea in ideas.OrderBy(i => i.SubmittedAt))
        {
            result.Add(await MapToDto(idea));
        }

        return result;
    }

    public async Task<IEnumerable<IdeaDto>> GetIdeasBySessionAsync(Guid sessionId)
    {
        var ideas = await _unitOfWork.Ideas.FindAsync(i => i.SessionId == sessionId);
        var result = new List<IdeaDto>();

        foreach (var idea in ideas.OrderBy(i => i.SubmittedAt))
        {
            result.Add(await MapToDto(idea));
        }

        return result;
    }

    public async Task<IEnumerable<IdeasByRoundDto>> GetIdeasGroupedByRoundAsync(Guid sessionId)
    {
        var rounds = await _unitOfWork.Rounds.FindAsync(r => r.SessionId == sessionId);
        var result = new List<IdeasByRoundDto>();

        foreach (var round in rounds.OrderBy(r => r.RoundNumber))
        {
            var ideas = await GetIdeasByRoundAsync(round.Id);
            result.Add(new IdeasByRoundDto
            {
                RoundNumber = round.RoundNumber,
                RoundId = round.Id,
                Ideas = ideas.ToList()
            });
        }

        return result;
    }

    public async Task<IEnumerable<IdeaDto>> GetUserIdeasInRoundAsync(Guid roundId, Guid userId)
    {
        var ideas = await _unitOfWork.Ideas.FindAsync(
            i => i.RoundId == roundId && i.UserId == userId);
        var result = new List<IdeaDto>();

        foreach (var idea in ideas.OrderBy(i => i.OrderInRound))
        {
            result.Add(await MapToDto(idea));
        }

        return result;
    }

    public async Task<int> GetUserIdeaCountInRoundAsync(Guid roundId, Guid userId)
    {
        return await _unitOfWork.Ideas.CountAsync(
            i => i.RoundId == roundId && i.UserId == userId);
    }

    public async Task<bool> ValidateIdeaLimitAsync(Guid roundId, Guid userId)
    {
        var count = await GetUserIdeaCountInRoundAsync(roundId, userId);
        return count < MaxIdeasPerRound;
    }

    public async Task<bool> CanUserSubmitAsync(Guid sessionId, Guid userId)
    {
        var session = await _unitOfWork.BrainstormingSessions.GetByIdAsync(sessionId);
        if (session == null || session.Status != SessionStatus.InProgress)
        {
            return false;
        }

        // Check if user is team member
        var isMember = await _teamService.IsMemberAsync(session.TeamId, userId);
        if (!isMember)
        {
            return false;
        }

        // Get current round
        var currentRound = await _unitOfWork.Rounds.FirstOrDefaultAsync(
            r => r.SessionId == sessionId && r.RoundNumber == session.CurrentRound);
        if (currentRound == null || currentRound.Status != SessionStatus.InProgress)
        {
            return false;
        }

        // Check idea limit
        return await ValidateIdeaLimitAsync(currentRound.Id, userId);
    }

    public async Task DeleteIdeaAsync(Guid ideaId, Guid userId)
    {
        var idea = await _unitOfWork.Ideas.GetByIdAsync(ideaId);
        if (idea == null)
        {
            throw new InvalidOperationException("Idea not found");
        }

        // Verify ownership
        if (idea.UserId != userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user?.Role != UserRole.EventManager)
            {
                throw new UnauthorizedAccessException("You can only delete your own ideas");
            }
        }

        // Verify round is still active
        var round = await _unitOfWork.Rounds.GetByIdAsync(idea.RoundId);
        if (round?.Status == SessionStatus.Completed)
        {
            throw new InvalidOperationException("Cannot delete ideas after round has ended");
        }

        await _unitOfWork.Ideas.DeleteAsync(idea);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<IdeaDto> MapToDto(Idea idea)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(idea.UserId);
        var round = await _unitOfWork.Rounds.GetByIdAsync(idea.RoundId);

        return new IdeaDto
        {
            Id = idea.Id,
            RoundId = idea.RoundId,
            RoundNumber = round?.RoundNumber ?? 0,
            SessionId = idea.SessionId,
            UserId = idea.UserId,
            UserFullName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
            Content = idea.Content,
            OrderInRound = idea.OrderInRound,
            IsAIGenerated = idea.IsAIGenerated,
            AIAnnotation = idea.AIAnnotation,
            SubmittedAt = idea.SubmittedAt
        };
    }
}
