using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BrainstormingApp.Application.Common;
using BrainstormingApp.Application.Common.Exceptions;
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
    public async Task<ActionResult<ApiResponse<SessionAnalyticsDto>>> GetSessionAnalytics(Guid sessionId)
    {
        var analytics = await _reportingService.GetSessionAnalyticsAsync(sessionId);
        return Ok(ApiResponse<SessionAnalyticsDto>.SuccessResponse(analytics));
    }

    /// <summary>
    /// Get analytics for an event
    /// </summary>
    [HttpGet("event/{eventId}/analytics")]
    public async Task<ActionResult<ApiResponse<EventAnalyticsDto>>> GetEventAnalytics(Guid eventId)
    {
        var analytics = await _reportingService.GetEventAnalyticsAsync(eventId);
        return Ok(ApiResponse<EventAnalyticsDto>.SuccessResponse(analytics));
    }

    /// <summary>
    /// Get session audit logs
    /// </summary>
    [HttpGet("session/{sessionId}/logs")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SessionLogDto>>>> GetSessionLogs(Guid sessionId)
    {
        var logs = await _reportingService.GetSessionLogsAsync(sessionId);
        return Ok(ApiResponse<IEnumerable<SessionLogDto>>.SuccessResponse(logs));
    }

    /// <summary>
    /// Export session report as PDF
    /// </summary>
    [HttpGet("session/{sessionId}/export/pdf")]
    public async Task<ActionResult> ExportSessionPdf(Guid sessionId)
    {
        var pdfBytes = await _reportingService.ExportSessionToPdfAsync(sessionId);
        return File(pdfBytes, "application/pdf", $"session_{sessionId}_report.pdf");
    }

    /// <summary>
    /// Export session report as Excel/CSV
    /// </summary>
    [HttpGet("session/{sessionId}/export/excel")]
    public async Task<ActionResult> ExportSessionExcel(Guid sessionId)
    {
        var excelBytes = await _reportingService.ExportSessionToExcelAsync(sessionId);
        return File(excelBytes, "text/csv", $"session_{sessionId}_report.csv");
    }

    /// <summary>
    /// Export event report as PDF
    /// </summary>
    [HttpGet("event/{eventId}/export/pdf")]
    public async Task<ActionResult> ExportEventPdf(Guid eventId)
    {
        var pdfBytes = await _reportingService.ExportEventToPdfAsync(eventId);
        return File(pdfBytes, "application/pdf", $"event_{eventId}_report.pdf");
    }

    /// <summary>
    /// Export event report as Excel/CSV
    /// </summary>
    [HttpGet("event/{eventId}/export/excel")]
    public async Task<ActionResult> ExportEventExcel(Guid eventId)
    {
        var excelBytes = await _reportingService.ExportEventToExcelAsync(eventId);
        return File(excelBytes, "text/csv", $"event_{eventId}_report.csv");
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
