using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult<IEnumerable<TopicDetailDto>>> GetByEvent(Guid eventId)
    {
        try
        {
            var topics = await _topicService.GetTopicsByEventAsync(eventId);
            return Ok(topics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific topic by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TopicDetailDto>> GetById(Guid id)
    {
        try
        {
            var topic = await _topicService.GetTopicByIdAsync(id);
            if (topic == null)
            {
                return NotFound(new { message = "Topic not found" });
            }
            return Ok(topic);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new topic
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TopicDetailDto>> Create([FromBody] CreateTopicDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var topic = await _topicService.CreateTopicAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = topic.Id }, topic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing topic
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TopicDetailDto>> Update(Guid id, [FromBody] UpdateTopicDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var topic = await _topicService.UpdateTopicAsync(id, dto, userId);
            return Ok(topic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a topic
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _topicService.DeleteTopicAsync(id, userId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Change topic status
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<TopicDetailDto>> ChangeStatus(Guid id, [FromBody] TopicStatus status)
    {
        try
        {
            var userId = GetCurrentUserId();
            var topic = await _topicService.ChangeStatusAsync(id, status, userId);
            return Ok(topic);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }

        return userId;
    }
}
