using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BrainstormingApp.Application.DTOs.Report;
using BrainstormingApp.Application.Interfaces;

namespace BrainstormingApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportingService _reportingService;

    public ReportsController(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    /// <summary>
    /// Get analytics for a session
    /// </summary>
    [HttpGet("session/{sessionId}/analytics")]
    public async Task<ActionResult<SessionAnalyticsDto>> GetSessionAnalytics(Guid sessionId)
    {
        try
        {
            var analytics = await _reportingService.GetSessionAnalyticsAsync(sessionId);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get analytics for an event
    /// </summary>
    [HttpGet("event/{eventId}/analytics")]
    public async Task<ActionResult<EventAnalyticsDto>> GetEventAnalytics(Guid eventId)
    {
        try
        {
            var analytics = await _reportingService.GetEventAnalyticsAsync(eventId);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get session audit logs
    /// </summary>
    [HttpGet("session/{sessionId}/logs")]
    public async Task<ActionResult<IEnumerable<SessionLogDto>>> GetSessionLogs(Guid sessionId)
    {
        try
        {
            var logs = await _reportingService.GetSessionLogsAsync(sessionId);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Export session report as text (PDF placeholder - use QuestPDF for real PDF)
    /// </summary>
    [HttpGet("session/{sessionId}/export/pdf")]
    public async Task<ActionResult> ExportSessionPdf(Guid sessionId)
    {
        try
        {
            var pdfBytes = await _reportingService.ExportSessionToPdfAsync(sessionId);
            return File(pdfBytes, "text/plain", $"session_{sessionId}_report.txt");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Export session report as CSV
    /// </summary>
    [HttpGet("session/{sessionId}/export/excel")]
    public async Task<ActionResult> ExportSessionExcel(Guid sessionId)
    {
        try
        {
            var excelBytes = await _reportingService.ExportSessionToExcelAsync(sessionId);
            return File(excelBytes, "text/csv; charset=utf-8", $"session_{sessionId}_ideas.csv");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Export event report as text (PDF placeholder)
    /// </summary>
    [HttpGet("event/{eventId}/export/pdf")]
    public async Task<ActionResult> ExportEventPdf(Guid eventId)
    {
        try
        {
            var pdfBytes = await _reportingService.ExportEventToPdfAsync(eventId);
            return File(pdfBytes, "text/plain", $"event_{eventId}_report.txt");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Export event report as CSV
    /// </summary>
    [HttpGet("event/{eventId}/export/excel")]
    public async Task<ActionResult> ExportEventExcel(Guid eventId)
    {
        try
        {
            var excelBytes = await _reportingService.ExportEventToExcelAsync(eventId);
            return File(excelBytes, "text/csv; charset=utf-8", $"event_{eventId}_sessions.csv");
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
