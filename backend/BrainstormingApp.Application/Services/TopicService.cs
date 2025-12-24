using BrainstormingApp.Application.DTOs.Topic;
using BrainstormingApp.Application.Interfaces;
using BrainstormingApp.Core.Entities;
using BrainstormingApp.Core.Enums;
using BrainstormingApp.Core.Interfaces;

namespace BrainstormingApp.Application.Services;

public class TopicService : ITopicService
{
    private readonly IUnitOfWork _unitOfWork;

    public TopicService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<TopicDetailDto>> GetTopicsByEventAsync(Guid eventId)
    {
        var topics = await _unitOfWork.Topics.FindAsync(t => t.EventId == eventId);
        var result = new List<TopicDetailDto>();

        foreach (var topic in topics)
        {
            result.Add(await MapToDetailDto(topic));
        }

        return result.OrderByDescending(t => t.CreatedAt);
    }

    public async Task<TopicDetailDto?> GetTopicByIdAsync(Guid topicId)
    {
        var topic = await _unitOfWork.Topics.GetByIdAsync(topicId);
        if (topic == null) return null;

        return await MapToDetailDto(topic);
    }

    public async Task<TopicDetailDto> CreateTopicAsync(CreateTopicDto dto, Guid userId)
    {
        // Verify event exists
        var evt = await _unitOfWork.Events.GetByIdAsync(dto.EventId);
        if (evt == null)
        {
            throw new InvalidOperationException("Event not found");
        }

        // Verify user is a participant or event manager
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var isParticipant = await _unitOfWork.EventParticipants.ExistsAsync(
            p => p.EventId == dto.EventId && p.UserId == userId);

        if (!isParticipant && user.Role != UserRole.EventManager)
        {
            throw new UnauthorizedAccessException("Only event participants can create topics");
        }

        var topic = new Topic
        {
            Id = Guid.NewGuid(),
            EventId = dto.EventId,
            Title = dto.Title,
            Description = dto.Description,
            Status = TopicStatus.Open,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Topics.AddAsync(topic);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDetailDto(topic);
    }

    public async Task<TopicDetailDto> UpdateTopicAsync(Guid topicId, UpdateTopicDto dto, Guid userId)
    {
        var topic = await _unitOfWork.Topics.GetByIdAsync(topicId);
        if (topic == null)
        {
            throw new InvalidOperationException("Topic not found");
        }

        // Verify user has permission
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        var evt = await _unitOfWork.Events.GetByIdAsync(topic.EventId);

        if (user?.Role != UserRole.EventManager && evt?.CreatedById != userId)
        {
            var isParticipant = await _unitOfWork.EventParticipants.ExistsAsync(
                p => p.EventId == topic.EventId && p.UserId == userId && p.Role == UserRole.EventManager);
            if (!isParticipant)
            {
                throw new UnauthorizedAccessException("Only EventManagers can update topics");
            }
        }

        topic.Title = dto.Title;
        topic.Description = dto.Description;
        if (dto.Status.HasValue)
        {
            topic.Status = dto.Status.Value;
        }
        topic.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Topics.UpdateAsync(topic);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDetailDto(topic);
    }

    public async Task DeleteTopicAsync(Guid topicId, Guid userId)
    {
        var topic = await _unitOfWork.Topics.GetByIdAsync(topicId);
        if (topic == null)
        {
            throw new InvalidOperationException("Topic not found");
        }

        // Check if there are active sessions using this topic
        var activeSessions = await _unitOfWork.BrainstormingSessions.FindAsync(
            s => s.TopicId == topicId && s.Status == SessionStatus.InProgress);
        if (activeSessions.Any())
        {
            throw new InvalidOperationException("Cannot delete topic with active sessions");
        }

        await _unitOfWork.Topics.DeleteAsync(topic);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<TopicDetailDto> ChangeStatusAsync(Guid topicId, TopicStatus status, Guid userId)
    {
        var topic = await _unitOfWork.Topics.GetByIdAsync(topicId);
        if (topic == null)
        {
            throw new InvalidOperationException("Topic not found");
        }

        topic.Status = status;
        topic.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Topics.UpdateAsync(topic);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDetailDto(topic);
    }

    private async Task<TopicDetailDto> MapToDetailDto(Topic topic)
    {
        var evt = await _unitOfWork.Events.GetByIdAsync(topic.EventId);
        var sessionCount = await _unitOfWork.BrainstormingSessions.CountAsync(s => s.TopicId == topic.Id);

        return new TopicDetailDto
        {
            Id = topic.Id,
            EventId = topic.EventId,
            EventName = evt?.Name ?? "Unknown",
            Title = topic.Title,
            Description = topic.Description,
            Status = topic.Status,
            CreatedAt = topic.CreatedAt,
            UpdatedAt = topic.UpdatedAt,
            SessionCount = sessionCount
        };
    }
}
