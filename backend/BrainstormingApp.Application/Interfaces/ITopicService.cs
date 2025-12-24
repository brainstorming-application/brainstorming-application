using BrainstormingApp.Application.DTOs.Topic;
using BrainstormingApp.Core.Enums;

namespace BrainstormingApp.Application.Interfaces;

public interface ITopicService
{
    Task<IEnumerable<TopicDetailDto>> GetTopicsByEventAsync(Guid eventId);
    Task<TopicDetailDto?> GetTopicByIdAsync(Guid topicId);
    Task<TopicDetailDto> CreateTopicAsync(CreateTopicDto dto, Guid userId);
    Task<TopicDetailDto> UpdateTopicAsync(Guid topicId, UpdateTopicDto dto, Guid userId);
    Task DeleteTopicAsync(Guid topicId, Guid userId);
    Task<TopicDetailDto> ChangeStatusAsync(Guid topicId, TopicStatus status, Guid userId);
}
