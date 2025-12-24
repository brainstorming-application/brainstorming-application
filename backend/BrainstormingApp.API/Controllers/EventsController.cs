using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BrainstormingApp.Application.DTOs.Event;
using BrainstormingApp.Application.Interfaces;
using BrainstormingApp.Core.Enums;

namespace BrainstormingApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    /// <summary>
    /// Get all events
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventDetailDto>>> GetAll()
    {
        try
        {
            var events = await _eventService.GetAllEventsAsync();
            return Ok(events);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get events for current user (created or participating)
    /// </summary>
    [HttpGet("my-events")]
    public async Task<ActionResult<IEnumerable<EventDetailDto>>> GetMyEvents()
    {
        try
        {
            var userId = GetCurrentUserId();
            var events = await _eventService.GetEventsByUserAsync(userId);
            return Ok(events);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get event by ID with full details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<EventDetailDto>> GetById(Guid id)
    {
        try
        {
            var eventDetail = await _eventService.GetEventByIdAsync(id);
            if (eventDetail == null)
            {
                return NotFound(new { message = "Event not found" });
            }
            return Ok(eventDetail);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new event (EventManager only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "EventManager")]
    public async Task<ActionResult<EventDetailDto>> Create([FromBody] CreateEventDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var eventDetail = await _eventService.CreateEventAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = eventDetail.Id }, eventDetail);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing event (EventManager only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "EventManager")]
    public async Task<ActionResult<EventDetailDto>> Update(Guid id, [FromBody] UpdateEventDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var eventDetail = await _eventService.UpdateEventAsync(id, dto, userId);
            return Ok(eventDetail);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete an event (EventManager only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "EventManager")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _eventService.DeleteEventAsync(id, userId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Start an event (change status to Active)
    /// </summary>
    [HttpPost("{id}/start")]
    [Authorize(Roles = "EventManager")]
    public async Task<ActionResult<EventDetailDto>> Start(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var eventDetail = await _eventService.ChangeStatusAsync(id, EventStatus.Active, userId);
            return Ok(eventDetail);
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
    /// Complete an event
    /// </summary>
    [HttpPost("{id}/complete")]
    [Authorize(Roles = "EventManager")]
    public async Task<ActionResult<EventDetailDto>> Complete(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var eventDetail = await _eventService.ChangeStatusAsync(id, EventStatus.Completed, userId);
            return Ok(eventDetail);
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
    /// Cancel an event
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "EventManager")]
    public async Task<ActionResult<EventDetailDto>> Cancel(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var eventDetail = await _eventService.ChangeStatusAsync(id, EventStatus.Cancelled, userId);
            return Ok(eventDetail);
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
    /// Add a participant to an event
    /// </summary>
    [HttpPost("{id}/participants")]
    [Authorize(Roles = "EventManager")]
    public async Task<ActionResult<EventParticipantDto>> AddParticipant(Guid id, [FromBody] AddParticipantDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var participant = await _eventService.AddParticipantAsync(id, dto, userId);
            return Ok(participant);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove a participant from an event
    /// </summary>
    [HttpDelete("{eventId}/participants/{participantUserId}")]
    [Authorize(Roles = "EventManager")]
    public async Task<ActionResult> RemoveParticipant(Guid eventId, Guid participantUserId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _eventService.RemoveParticipantAsync(eventId, participantUserId, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all participants in an event
    /// </summary>
    [HttpGet("{id}/participants")]
    public async Task<ActionResult<IEnumerable<EventParticipantDto>>> GetParticipants(Guid id)
    {
        try
        {
            var participants = await _eventService.GetParticipantsAsync(id);
            return Ok(participants);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Check if current user is a participant in an event
    /// </summary>
    [HttpGet("{id}/is-participant")]
    public async Task<ActionResult> IsParticipant(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var isParticipant = await _eventService.IsParticipantAsync(id, userId);
            return Ok(new { isParticipant });
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
