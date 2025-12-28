using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BrainstormingApp.Application.Common;
using BrainstormingApp.Application.Common.Exceptions;
using BrainstormingApp.Application.DTOs.Topic;
using BrainstormingApp.Application.Interfaces;
using BrainstormingApp.Core.Enums;

namespace BrainstormingApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TopicsController : ControllerBase
{
    private readonly ITopicService _topicService;

    public TopicsController(ITopicService topicService)
    {
        _topicService = topicService;
    }

    /// <summary>
    /// Get all topics for an event
    /// </summary>
    [HttpGet("event/{eventId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TopicDetailDto>>>> GetByEvent(Guid eventId)
    {
        var topics = await _topicService.GetTopicsByEventAsync(eventId);
        return Ok(ApiResponse<IEnumerable<TopicDetailDto>>.SuccessResponse(topics));
    }

    /// <summary>
    /// Get a specific topic by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TopicDetailDto>>> GetById(Guid id)
    {
        var topic = await _topicService.GetTopicByIdAsync(id);
        if (topic == null)
        {
            throw new NotFoundException("Topic", id);
        }
        return Ok(ApiResponse<TopicDetailDto>.SuccessResponse(topic));
    }

    /// <summary>
    /// Create a new topic
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TopicDetailDto>>> Create([FromBody] CreateTopicDto dto)
    {
        var userId = GetCurrentUserId();
        var topic = await _topicService.CreateTopicAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = topic.Id },
            ApiResponse<TopicDetailDto>.SuccessResponse(topic, "Topic created successfully"));
    }

    /// <summary>
    /// Update an existing topic
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<TopicDetailDto>>> Update(Guid id, [FromBody] UpdateTopicDto dto)
    {
        var userId = GetCurrentUserId();
        var topic = await _topicService.UpdateTopicAsync(id, dto, userId);
        return Ok(ApiResponse<TopicDetailDto>.SuccessResponse(topic, "Topic updated successfully"));
    }

    /// <summary>
    /// Delete a topic
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        await _topicService.DeleteTopicAsync(id, userId);
        return Ok(ApiResponse.SuccessResult("Topic deleted successfully"));
    }

    /// <summary>
    /// Change topic status
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ApiResponse<TopicDetailDto>>> ChangeStatus(Guid id, [FromBody] ChangeTopicStatusDto dto)
    {
        var userId = GetCurrentUserId();
        var topic = await _topicService.ChangeStatusAsync(id, dto.Status, userId);
        return Ok(ApiResponse<TopicDetailDto>.SuccessResponse(topic, "Topic status updated successfully"));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Invalid user token");
        }

        return userId;
    }
}
