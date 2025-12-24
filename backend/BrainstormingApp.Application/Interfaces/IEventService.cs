using BrainstormingApp.Application.DTOs.Event;
using BrainstormingApp.Core.Enums;

namespace BrainstormingApp.Application.Interfaces;

public interface IEventService
{
    Task<IEnumerable<EventDetailDto>> GetAllEventsAsync();
    Task<EventDetailDto?> GetEventByIdAsync(Guid eventId);
    Task<IEnumerable<EventDetailDto>> GetEventsByUserAsync(Guid userId);
    Task<EventDetailDto> CreateEventAsync(CreateEventDto dto, Guid createdById);
    Task<EventDetailDto> UpdateEventAsync(Guid eventId, UpdateEventDto dto, Guid userId);
    Task DeleteEventAsync(Guid eventId, Guid userId);
    Task<EventDetailDto> ChangeStatusAsync(Guid eventId, EventStatus status, Guid userId);

    // Participant management
    Task<IEnumerable<EventParticipantDto>> GetParticipantsAsync(Guid eventId);
    Task<EventParticipantDto> AddParticipantAsync(Guid eventId, AddParticipantDto dto, Guid addedById);
    Task RemoveParticipantAsync(Guid eventId, Guid userId, Guid removedById);
    Task<bool> IsParticipantAsync(Guid eventId, Guid userId);
}
