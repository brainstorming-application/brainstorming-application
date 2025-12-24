using BrainstormingApp.Application.DTOs.Event;
using BrainstormingApp.Application.Interfaces;
using BrainstormingApp.Core.Entities;
using BrainstormingApp.Core.Enums;
using BrainstormingApp.Core.Interfaces;

namespace BrainstormingApp.Application.Services;

public class EventService : IEventService
{
    private readonly IUnitOfWork _unitOfWork;

    public EventService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<EventDetailDto>> GetAllEventsAsync()
    {
        var events = await _unitOfWork.Events.GetAllAsync();
        var result = new List<EventDetailDto>();

        foreach (var evt in events)
        {
            result.Add(await MapToDetailDto(evt));
        }

        return result.OrderByDescending(e => e.CreatedAt);
    }

    public async Task<EventDetailDto?> GetEventByIdAsync(Guid eventId)
    {
        var evt = await _unitOfWork.Events.GetByIdAsync(eventId);
        if (evt == null) return null;

        return await MapToDetailDto(evt);
    }

    public async Task<IEnumerable<EventDetailDto>> GetEventsByUserAsync(Guid userId)
    {
        var participations = await _unitOfWork.EventParticipants.FindAsync(p => p.UserId == userId);
        var result = new List<EventDetailDto>();

        foreach (var participation in participations)
        {
            var evt = await _unitOfWork.Events.GetByIdAsync(participation.EventId);
            if (evt != null)
            {
                result.Add(await MapToDetailDto(evt));
            }
        }

        return result.OrderByDescending(e => e.CreatedAt);
    }

    public async Task<EventDetailDto> CreateEventAsync(CreateEventDto dto, Guid createdById)
    {
        // Validate dates
        if (dto.EndDate <= dto.StartDate)
        {
            throw new InvalidOperationException("End date must be after start date");
        }

        // Verify user exists and has EventManager role
        var user = await _unitOfWork.Users.GetByIdAsync(createdById);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (user.Role != UserRole.EventManager)
        {
            throw new UnauthorizedAccessException("Only EventManagers can create events");
        }

        var evt = new Event
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = EventStatus.Planned,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Events.AddAsync(evt);

        // Add creator as participant with EventManager role
        var participant = new EventParticipant
        {
            Id = Guid.NewGuid(),
            EventId = evt.Id,
            UserId = createdById,
            Role = UserRole.EventManager,
            JoinedAt = DateTime.UtcNow
        };

        await _unitOfWork.EventParticipants.AddAsync(participant);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDetailDto(evt);
    }

    public async Task<EventDetailDto> UpdateEventAsync(Guid eventId, UpdateEventDto dto, Guid userId)
    {
        var evt = await _unitOfWork.Events.GetByIdAsync(eventId);
        if (evt == null)
        {
            throw new InvalidOperationException("Event not found");
        }

        // Check authorization
        if (evt.CreatedById != userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user?.Role != UserRole.EventManager)
            {
                throw new UnauthorizedAccessException("Only the event creator or EventManagers can update events");
            }
        }

        // Validate dates
        if (dto.EndDate <= dto.StartDate)
        {
            throw new InvalidOperationException("End date must be after start date");
        }

        evt.Name = dto.Name;
        evt.Description = dto.Description;
        evt.StartDate = dto.StartDate;
        evt.EndDate = dto.EndDate;
        if (dto.Status.HasValue)
        {
            evt.Status = dto.Status.Value;
        }
        evt.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Events.UpdateAsync(evt);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDetailDto(evt);
    }

    public async Task DeleteEventAsync(Guid eventId, Guid userId)
    {
        var evt = await _unitOfWork.Events.GetByIdAsync(eventId);
        if (evt == null)
        {
            throw new InvalidOperationException("Event not found");
        }

        // Check authorization
        if (evt.CreatedById != userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user?.Role != UserRole.EventManager)
            {
                throw new UnauthorizedAccessException("Only the event creator or EventManagers can delete events");
            }
        }

        // Check if there are active sessions
        var teams = await _unitOfWork.Teams.FindAsync(t => t.EventId == eventId);
        foreach (var team in teams)
        {
            var activeSessions = await _unitOfWork.BrainstormingSessions.FindAsync(
                s => s.TeamId == team.Id && s.Status == SessionStatus.InProgress);
            if (activeSessions.Any())
            {
                throw new InvalidOperationException("Cannot delete event with active sessions");
            }
        }

        await _unitOfWork.Events.DeleteAsync(evt);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<EventDetailDto> ChangeStatusAsync(Guid eventId, EventStatus status, Guid userId)
    {
        var evt = await _unitOfWork.Events.GetByIdAsync(eventId);
        if (evt == null)
        {
            throw new InvalidOperationException("Event not found");
        }

        // Validate state transition
        if (!IsValidStatusTransition(evt.Status, status))
        {
            throw new InvalidOperationException($"Cannot transition from {evt.Status} to {status}");
        }

        evt.Status = status;
        evt.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Events.UpdateAsync(evt);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDetailDto(evt);
    }

    public async Task<IEnumerable<EventParticipantDto>> GetParticipantsAsync(Guid eventId)
    {
        var participants = await _unitOfWork.EventParticipants.FindAsync(p => p.EventId == eventId);
        var result = new List<EventParticipantDto>();

        foreach (var participant in participants)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(participant.UserId);
            if (user != null)
            {
                result.Add(new EventParticipantDto
                {
                    Id = participant.Id,
                    EventId = participant.EventId,
                    UserId = participant.UserId,
                    UserFullName = $"{user.FirstName} {user.LastName}",
                    UserEmail = user.Email,
                    Role = participant.Role,
                    JoinedAt = participant.JoinedAt
                });
            }
        }

        return result;
    }

    public async Task<EventParticipantDto> AddParticipantAsync(Guid eventId, AddParticipantDto dto, Guid addedById)
    {
        var evt = await _unitOfWork.Events.GetByIdAsync(eventId);
        if (evt == null)
        {
            throw new InvalidOperationException("Event not found");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Check if already a participant
        var existing = await _unitOfWork.EventParticipants.FirstOrDefaultAsync(
            p => p.EventId == eventId && p.UserId == dto.UserId);
        if (existing != null)
        {
            throw new InvalidOperationException("User is already a participant");
        }

        var participant = new EventParticipant
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = dto.UserId,
            Role = dto.Role,
            JoinedAt = DateTime.UtcNow
        };

        await _unitOfWork.EventParticipants.AddAsync(participant);
        await _unitOfWork.SaveChangesAsync();

        return new EventParticipantDto
        {
            Id = participant.Id,
            EventId = participant.EventId,
            UserId = participant.UserId,
            UserFullName = $"{user.FirstName} {user.LastName}",
            UserEmail = user.Email,
            Role = participant.Role,
            JoinedAt = participant.JoinedAt
        };
    }

    public async Task RemoveParticipantAsync(Guid eventId, Guid userId, Guid removedById)
    {
        var participant = await _unitOfWork.EventParticipants.FirstOrDefaultAsync(
            p => p.EventId == eventId && p.UserId == userId);

        if (participant == null)
        {
            throw new InvalidOperationException("Participant not found");
        }

        // Cannot remove the event creator
        var evt = await _unitOfWork.Events.GetByIdAsync(eventId);
        if (evt != null && evt.CreatedById == userId)
        {
            throw new InvalidOperationException("Cannot remove the event creator");
        }

        await _unitOfWork.EventParticipants.DeleteAsync(participant);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> IsParticipantAsync(Guid eventId, Guid userId)
    {
        return await _unitOfWork.EventParticipants.ExistsAsync(
            p => p.EventId == eventId && p.UserId == userId);
    }

    private async Task<EventDetailDto> MapToDetailDto(Event evt)
    {
        var creator = await _unitOfWork.Users.GetByIdAsync(evt.CreatedById);
        var topicCount = await _unitOfWork.Topics.CountAsync(t => t.EventId == evt.Id);
        var teamCount = await _unitOfWork.Teams.CountAsync(t => t.EventId == evt.Id);
        var participantCount = await _unitOfWork.EventParticipants.CountAsync(p => p.EventId == evt.Id);

        return new EventDetailDto
        {
            Id = evt.Id,
            Name = evt.Name,
            Description = evt.Description,
            StartDate = evt.StartDate,
            EndDate = evt.EndDate,
            Status = evt.Status,
            CreatedById = evt.CreatedById,
            CreatedByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown",
            CreatedAt = evt.CreatedAt,
            UpdatedAt = evt.UpdatedAt,
            TopicCount = topicCount,
            TeamCount = teamCount,
            ParticipantCount = participantCount
        };
    }

    private bool IsValidStatusTransition(EventStatus current, EventStatus next)
    {
        return (current, next) switch
        {
            (EventStatus.Planned, EventStatus.Active) => true,
            (EventStatus.Planned, EventStatus.Cancelled) => true,
            (EventStatus.Active, EventStatus.Completed) => true,
            (EventStatus.Active, EventStatus.Cancelled) => true,
            _ => false
        };
    }
}
