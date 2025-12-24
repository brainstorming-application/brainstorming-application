using BrainstormingApp.Application.DTOs.Report;

namespace BrainstormingApp.Application.Interfaces;

public interface IReportingService
{
    // Analytics
    Task<SessionAnalyticsDto> GetSessionAnalyticsAsync(Guid sessionId);
    Task<EventAnalyticsDto> GetEventAnalyticsAsync(Guid eventId);

    // Session Logging
    Task LogActionAsync(Guid sessionId, Guid? userId, string action, string? details = null);
    Task<IEnumerable<SessionLogDto>> GetSessionLogsAsync(Guid sessionId);

    // Export
    Task<byte[]> ExportSessionToPdfAsync(Guid sessionId);
    Task<byte[]> ExportSessionToExcelAsync(Guid sessionId);
    Task<byte[]> ExportEventToPdfAsync(Guid eventId);
    Task<byte[]> ExportEventToExcelAsync(Guid eventId);
}
