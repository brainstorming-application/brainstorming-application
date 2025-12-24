using BrainstormingApp.Application.DTOs.Report;
using BrainstormingApp.Application.Interfaces;
using BrainstormingApp.Core.Entities;
using BrainstormingApp.Core.Enums;
using BrainstormingApp.Core.Interfaces;

namespace BrainstormingApp.Application.Services;

public class ReportingService : IReportingService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SessionAnalyticsDto> GetSessionAnalyticsAsync(Guid sessionId)
    {
        var session = await _unitOfWork.BrainstormingSessions.GetByIdAsync(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Session not found");
        }

        var team = await _unitOfWork.Teams.GetByIdAsync(session.TeamId);
        var topic = await _unitOfWork.Topics.GetByIdAsync(session.TopicId);
        var rounds = await _unitOfWork.Rounds.FindAsync(r => r.SessionId == sessionId);
        var ideas = await _unitOfWork.Ideas.FindAsync(i => i.SessionId == sessionId);
        var members = await _unitOfWork.TeamMembers.FindAsync(m => m.TeamId == session.TeamId);

        var ideaList = ideas.ToList();
        var memberCount = members.Count();
        var completedRounds = rounds.Count(r => r.Status == SessionStatus.Completed);

        var aiGeneratedCount = ideaList.Count(i => i.IsAIGenerated);
        var humanGeneratedCount = ideaList.Count - aiGeneratedCount;

        var analytics = new SessionAnalyticsDto
        {
            SessionId = session.Id,
            TopicTitle = topic?.Title ?? "Unknown",
            TeamName = team?.Name ?? "Unknown",
            TotalIdeas = ideaList.Count,
            TotalRounds = session.TotalRounds,
            CompletedRounds = completedRounds,
            ParticipantCount = memberCount,
            AverageIdeasPerParticipant = memberCount > 0 ? (double)ideaList.Count / memberCount : 0,
            AverageIdeasPerRound = completedRounds > 0 ? (double)ideaList.Count / completedRounds : 0,
            AIGeneratedIdeas = aiGeneratedCount,
            HumanGeneratedIdeas = humanGeneratedCount,
            AIUsageRatio = ideaList.Count > 0 ? (double)aiGeneratedCount / ideaList.Count : 0,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            TotalDuration = session.StartedAt.HasValue && session.EndedAt.HasValue
                ? session.EndedAt.Value - session.StartedAt.Value
                : null,
            RoundBreakdown = new List<RoundAnalyticsDto>(),
            ParticipantBreakdown = new List<ParticipantAnalyticsDto>()
        };

        // Round breakdown
        foreach (var round in rounds.OrderBy(r => r.RoundNumber))
        {
            var roundIdeas = ideaList.Where(i => i.RoundId == round.Id).ToList();
            analytics.RoundBreakdown.Add(new RoundAnalyticsDto
            {
                RoundNumber = round.RoundNumber,
                IdeaCount = roundIdeas.Count,
                Duration = round.DurationSeconds.HasValue
                    ? TimeSpan.FromSeconds(round.DurationSeconds.Value)
                    : null,
                TopIdeas = roundIdeas.Take(3).Select(i => i.Content).ToList()
            });
        }

        // Participant breakdown
        foreach (var member in members)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(member.UserId);
            var userIdeas = ideaList.Where(i => i.UserId == member.UserId).ToList();

            analytics.ParticipantBreakdown.Add(new ParticipantAnalyticsDto
            {
                UserId = member.UserId,
                FullName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                IdeaCount = userIdeas.Count,
                AIAssistedIdeas = userIdeas.Count(i => i.IsAIGenerated || !string.IsNullOrEmpty(i.AIAnnotation))
            });
        }

        return analytics;
    }

    public async Task<EventAnalyticsDto> GetEventAnalyticsAsync(Guid eventId)
    {
        var evt = await _unitOfWork.Events.GetByIdAsync(eventId);
        if (evt == null)
        {
            throw new InvalidOperationException("Event not found");
        }

        var teams = await _unitOfWork.Teams.FindAsync(t => t.EventId == eventId);
        var topics = await _unitOfWork.Topics.FindAsync(t => t.EventId == eventId);
        var participants = await _unitOfWork.EventParticipants.FindAsync(p => p.EventId == eventId);

        var teamIds = teams.Select(t => t.Id).ToList();
        var sessions = new List<BrainstormingSession>();
        foreach (var teamId in teamIds)
        {
            var teamSessions = await _unitOfWork.BrainstormingSessions.FindAsync(s => s.TeamId == teamId);
            sessions.AddRange(teamSessions);
        }

        var totalIdeas = 0;
        var sessionAnalytics = new List<SessionAnalyticsDto>();

        foreach (var session in sessions)
        {
            var sessionIdeas = await _unitOfWork.Ideas.CountAsync(i => i.SessionId == session.Id);
            totalIdeas += sessionIdeas;

            try
            {
                var analytics = await GetSessionAnalyticsAsync(session.Id);
                sessionAnalytics.Add(analytics);
            }
            catch
            {
                // Skip if analytics fail
            }
        }

        var completedSessions = sessions.Count(s => s.Status == SessionStatus.Completed);

        return new EventAnalyticsDto
        {
            EventId = evt.Id,
            EventName = evt.Name,
            TotalSessions = sessions.Count,
            CompletedSessions = completedSessions,
            TotalIdeas = totalIdeas,
            TotalParticipants = participants.Count(),
            TotalTeams = teams.Count(),
            TotalTopics = topics.Count(),
            AverageIdeasPerSession = sessions.Count > 0 ? (double)totalIdeas / sessions.Count : 0,
            SessionBreakdown = sessionAnalytics
        };
    }

    public async Task LogActionAsync(Guid sessionId, Guid? userId, string action, string? details = null)
    {
        var log = new SessionLog
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            UserId = userId,
            Action = action,
            Details = details,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SessionLogs.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<SessionLogDto>> GetSessionLogsAsync(Guid sessionId)
    {
        var logs = await _unitOfWork.SessionLogs.FindAsync(l => l.SessionId == sessionId);
        var result = new List<SessionLogDto>();

        foreach (var log in logs.OrderByDescending(l => l.Timestamp))
        {
            var user = log.UserId.HasValue
                ? await _unitOfWork.Users.GetByIdAsync(log.UserId.Value)
                : null;

            result.Add(new SessionLogDto
            {
                Id = log.Id,
                SessionId = log.SessionId,
                UserId = log.UserId,
                UserFullName = user != null ? $"{user.FirstName} {user.LastName}" : null,
                Action = log.Action,
                Details = log.Details,
                Timestamp = log.Timestamp
            });
        }

        return result;
    }

    public async Task<byte[]> ExportSessionToPdfAsync(Guid sessionId)
    {
        var analytics = await GetSessionAnalyticsAsync(sessionId);
        var ideas = await _unitOfWork.Ideas.FindAsync(i => i.SessionId == sessionId);

        // Generate simple PDF content (in production, use a library like iTextSharp or QuestPDF)
        var content = GeneratePdfContent(analytics, ideas.ToList());
        return System.Text.Encoding.UTF8.GetBytes(content);
    }

    public async Task<byte[]> ExportSessionToExcelAsync(Guid sessionId)
    {
        var analytics = await GetSessionAnalyticsAsync(sessionId);
        var ideas = await _unitOfWork.Ideas.FindAsync(i => i.SessionId == sessionId);

        // Generate CSV content (in production, use a library like ClosedXML or EPPlus)
        var csv = GenerateCsvContent(analytics, ideas.ToList());
        return System.Text.Encoding.UTF8.GetBytes(csv);
    }

    public async Task<byte[]> ExportEventToPdfAsync(Guid eventId)
    {
        var analytics = await GetEventAnalyticsAsync(eventId);
        var content = GenerateEventPdfContent(analytics);
        return System.Text.Encoding.UTF8.GetBytes(content);
    }

    public async Task<byte[]> ExportEventToExcelAsync(Guid eventId)
    {
        var analytics = await GetEventAnalyticsAsync(eventId);
        var csv = GenerateEventCsvContent(analytics);
        return System.Text.Encoding.UTF8.GetBytes(csv);
    }

    private string GeneratePdfContent(SessionAnalyticsDto analytics, List<Idea> ideas)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("BRAINSTORMING SESSION REPORT");
        sb.AppendLine("===========================");
        sb.AppendLine();
        sb.AppendLine($"Topic: {analytics.TopicTitle}");
        sb.AppendLine($"Team: {analytics.TeamName}");
        sb.AppendLine($"Total Ideas: {analytics.TotalIdeas}");
        sb.AppendLine($"Rounds Completed: {analytics.CompletedRounds}/{analytics.TotalRounds}");
        sb.AppendLine($"Participants: {analytics.ParticipantCount}");
        sb.AppendLine($"Duration: {analytics.TotalDuration?.ToString(@"hh\:mm\:ss") ?? "N/A"}");
        sb.AppendLine();
        sb.AppendLine("IDEAS:");
        sb.AppendLine("------");

        foreach (var idea in ideas.OrderBy(i => i.SubmittedAt))
        {
            sb.AppendLine($"- {idea.Content}");
        }

        return sb.ToString();
    }

    private string GenerateCsvContent(SessionAnalyticsDto analytics, List<Idea> ideas)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Round,Order,Content,Submitted At,AI Generated");

        foreach (var idea in ideas.OrderBy(i => i.SubmittedAt))
        {
            var roundInfo = analytics.RoundBreakdown.FirstOrDefault(r =>
                r.TopIdeas.Contains(idea.Content));
            var roundNum = roundInfo?.RoundNumber ?? 0;

            sb.AppendLine($"{roundNum},{idea.OrderInRound},\"{idea.Content.Replace("\"", "\"\"")}\",{idea.SubmittedAt:yyyy-MM-dd HH:mm:ss},{idea.IsAIGenerated}");
        }

        return sb.ToString();
    }

    private string GenerateEventPdfContent(EventAnalyticsDto analytics)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("EVENT ANALYTICS REPORT");
        sb.AppendLine("======================");
        sb.AppendLine();
        sb.AppendLine($"Event: {analytics.EventName}");
        sb.AppendLine($"Total Sessions: {analytics.TotalSessions}");
        sb.AppendLine($"Completed Sessions: {analytics.CompletedSessions}");
        sb.AppendLine($"Total Ideas: {analytics.TotalIdeas}");
        sb.AppendLine($"Total Participants: {analytics.TotalParticipants}");
        sb.AppendLine($"Total Teams: {analytics.TotalTeams}");
        sb.AppendLine($"Average Ideas per Session: {analytics.AverageIdeasPerSession:F2}");

        return sb.ToString();
    }

    private string GenerateEventCsvContent(EventAnalyticsDto analytics)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Session,Topic,Team,Total Ideas,Completed Rounds,Participants");

        foreach (var session in analytics.SessionBreakdown)
        {
            sb.AppendLine($"\"{session.TopicTitle}\",\"{session.TeamName}\",{session.TotalIdeas},{session.CompletedRounds},{session.ParticipantCount}");
        }

        return sb.ToString();
    }
}
