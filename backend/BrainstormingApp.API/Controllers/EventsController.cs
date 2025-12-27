using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BrainstormingApp.Application.Common;
using BrainstormingApp.Application.Common.Exceptions;
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
    public async Task<ActionResult<ApiResponse<IEnumerable<EventDetailDto>>>> GetAll()
    {
        var events = await _eventService.GetAllEventsAsync();
        return Ok(ApiResponse<IEnumerable<EventDetailDto>>.SuccessResponse(events));
    }

    /// <summary>
    /// Get events for current user (created or participating)
    /// </summary>
    [HttpGet("my-events")]
    public async Task<ActionResult<ApiResponse<IEnumerable<EventDetailDto>>>> GetMyEvents()
    {
        var userId = GetCurrentUserId();
        var events = await _eventService.GetEventsByUserAsync(userId);
        return Ok(ApiResponse<IEnumerable<EventDetailDto>>.SuccessResponse(events));
    }

    /// <summary>
    /// Get event by ID with full details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<EventDetailDto>>> GetById(Guid id)
    {
        var eventDetail = await _eventService.GetEventByIdAsync(id);
        if (eventDetail == null)
        {
            throw new NotFoundException("Event", id);
        }
        return Ok(ApiResponse<EventDetailDto>.SuccessResponse(eventDetail));
    }

    /// <summary>
    /// Create a new event (EventManager only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "EventManager")]
    public async Task<ActionResult<ApiResponse<EventDetailDto>>> Create([FromBody] CreateEventDto dto)
    {
        var userId = GetCurrentUserId();
        var eventDetail = await _eventService.CreateEventAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = eventDetail.Id },
            ApiResponse<EventDetailDto>.SuccessResponse(eventDetail, "Event created successfully"));
    }

    /// <summary>
    /// Update an existing event (EventManager only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "EventManager")]
    public async Task<ActionResult<ApiResponse<EventDetailDto>>> Update(Guid id, [FromBody] UpdateEventDto dto)
    {
        var userId = GetCurrentUserId();
        var eventDetail = await _eventService.UpdateEventAsync(id, dto, userId);
        return Ok(ApiResponse<EventDetailDto>.SuccessResponse(eventDetail, "Event updated successfully"));
    }

    /// <summary>
    /// Delete an event (EventManager only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "EventManager")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        await _eventService.DeleteEventAsync(id, userId);
        return Ok(ApiResponse.SuccessResult("Event deleted successfully"));
    }

    /// <summary>
    /// Start an event (change status to Active)
    /// </summary>
    [HttpPost("{id}/start")]
    [Authorize(Roles = "EventManager")]
    public async Task<ActionResult<ApiResponse<EventDetailDto>>> Start(Guid id)
    {
        var userId = GetCurrentUserId();
        var eventDetail = await _eventService.ChangeStatusAsync(id, EventStatus.Active, userId);
        return Ok(ApiResponse<EventDetailDto>.SuccessResponse(eventDetail, "Event started successfully"));
    }

    /// <summary>
    /// Complete an event
    /// </summary>
    [HttpPost("{id}/complete")]
    [Authorize(Roles = "EventManager")]
    public async Task<ActionResult<ApiResponse<EventDetailDto>>> Complete(Guid id)
    {
        var userId = GetCurrentUserId();
        var eventDetail = await _eventService.ChangeStatusAsync(id, EventStatus.Completed, userId);
        return Ok(ApiResponse<EventDetailDto>.SuccessResponse(eventDetail, "Event completed successfully"));
    }

    /// <summary>
    /// Cancel an event
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "EventManager")]
    public async Task<ActionResult<ApiResponse<EventDetailDto>>> Cancel(Guid id)
    {
        var userId = GetCurrentUserId();
        var eventDetail = await _eventService.ChangeStatusAsync(id, EventStatus.Cancelled, userId);
        return Ok(ApiResponse<EventDetailDto>.SuccessResponse(eventDetail, "Event cancelled successfully"));
    }

    /// <summary>
    /// Add a participant to an event
    /// </summary>
    [HttpPost("{id}/participants")]
    [Authorize(Roles = "EventManager")]
    public async Task<ActionResult<ApiResponse<EventParticipantDto>>> AddParticipant(Guid id, [FromBody] AddParticipantDto dto)
    {
        var userId = GetCurrentUserId();
        var participant = await _eventService.AddParticipantAsync(id, dto, userId);
        return Ok(ApiResponse<EventParticipantDto>.SuccessResponse(participant, "Participant added successfully"));
    }

    /// <summary>
    /// Remove a participant from an event
    /// </summary>
    [HttpDelete("{eventId}/participants/{participantUserId}")]
    [Authorize(Roles = "EventManager")]
    public async Task<ActionResult<ApiResponse>> RemoveParticipant(Guid eventId, Guid participantUserId)
    {
        var userId = GetCurrentUserId();
        await _eventService.RemoveParticipantAsync(eventId, participantUserId, userId);
        return Ok(ApiResponse.SuccessResult("Participant removed successfully"));
    }

    /// <summary>
    /// Get all participants in an event
    /// </summary>
    [HttpGet("{id}/participants")]
    public async Task<ActionResult<ApiResponse<IEnumerable<EventParticipantDto>>>> GetParticipants(Guid id)
    {
        var participants = await _eventService.GetParticipantsAsync(id);
        return Ok(ApiResponse<IEnumerable<EventParticipantDto>>.SuccessResponse(participants));
    }

    /// <summary>
    /// Check if current user is a participant in an event
    /// </summary>
    [HttpGet("{id}/is-participant")]
    public async Task<ActionResult<ApiResponse<object>>> IsParticipant(Guid id)
    {
        var userId = GetCurrentUserId();
        var isParticipant = await _eventService.IsParticipantAsync(id, userId);
        return Ok(ApiResponse<object>.SuccessResponse(new { isParticipant }));
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
