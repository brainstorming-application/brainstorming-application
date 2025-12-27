using BrainstormingApp.Application.DTOs.Team;
using BrainstormingApp.Application.Interfaces;
using BrainstormingApp.Core.Entities;
using BrainstormingApp.Core.Enums;
using BrainstormingApp.Core.Interfaces;

namespace BrainstormingApp.Application.Services;

public class TeamService : ITeamService
{
    private readonly IUnitOfWork _unitOfWork;
    private const int MaxTeamSize = 6; // 6-3-5 method constraint

    public TeamService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<TeamDetailDto>> GetTeamsByEventAsync(Guid eventId)
    {
        var teams = await _unitOfWork.Teams.FindAsync(t => t.EventId == eventId);
        var result = new List<TeamDetailDto>();

        foreach (var team in teams)
        {
            result.Add(await MapToDetailDto(team));
        }

        return result.OrderByDescending(t => t.CreatedAt);
    }

    public async Task<IEnumerable<TeamDetailDto>> GetMyTeamsAsync(Guid userId)
    {
        // Get all team memberships for this user
        var memberships = await _unitOfWork.TeamMembers.FindAsync(m => m.UserId == userId);
        var teamIds = memberships.Select(m => m.TeamId).ToList();

        var result = new List<TeamDetailDto>();
        foreach (var teamId in teamIds)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
            if (team != null)
            {
                result.Add(await MapToDetailDto(team));
            }
        }

        return result.OrderByDescending(t => t.CreatedAt);
    }

    public async Task<TeamDetailDto?> GetTeamByIdAsync(Guid teamId)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
        if (team == null) return null;

        return await MapToDetailDto(team);
    }

    public async Task<TeamDetailDto> CreateTeamAsync(CreateTeamDto dto, Guid userId)
    {
        // Verify event exists
        var evt = await _unitOfWork.Events.GetByIdAsync(dto.EventId);
        if (evt == null)
        {
            throw new InvalidOperationException("Event not found");
        }

        // Verify user has permission (EventManager or TeamLeader)
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (user.Role != UserRole.EventManager && user.Role != UserRole.TeamLeader)
        {
            throw new UnauthorizedAccessException("Only EventManagers or TeamLeaders can create teams");
        }

        // Validate max members
        if (dto.MaxMembers > MaxTeamSize)
        {
            throw new InvalidOperationException($"Team cannot have more than {MaxTeamSize} members (6-3-5 method constraint)");
        }

        var team = new Team
        {
            Id = Guid.NewGuid(),
            EventId = dto.EventId,
            Name = dto.Name,
            Description = dto.Description,
            LeaderId = dto.LeaderId ?? userId,
            MaxMembers = dto.MaxMembers > 0 ? dto.MaxMembers : MaxTeamSize,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Teams.AddAsync(team);

        // Add leader as team member
        var leaderMember = new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = team.Id,
            UserId = team.LeaderId.Value,
            JoinedAt = DateTime.UtcNow
        };

        await _unitOfWork.TeamMembers.AddAsync(leaderMember);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDetailDto(team);
    }

    public async Task<TeamDetailDto> UpdateTeamAsync(Guid teamId, UpdateTeamDto dto, Guid userId)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
        if (team == null)
        {
            throw new InvalidOperationException("Team not found");
        }

        // Verify user has permission
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user?.Role != UserRole.EventManager && team.LeaderId != userId)
        {
            throw new UnauthorizedAccessException("Only EventManagers or team leaders can update teams");
        }

        team.Name = dto.Name;
        team.Description = dto.Description;
        if (dto.LeaderId.HasValue)
        {
            team.LeaderId = dto.LeaderId;
        }
        team.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Teams.UpdateAsync(team);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDetailDto(team);
    }

    public async Task DeleteTeamAsync(Guid teamId, Guid userId)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
        if (team == null)
        {
            throw new InvalidOperationException("Team not found");
        }

        // Check if there are active sessions
        var activeSessions = await _unitOfWork.BrainstormingSessions.FindAsync(
            s => s.TeamId == teamId && s.Status == SessionStatus.InProgress);
        if (activeSessions.Any())
        {
            throw new InvalidOperationException("Cannot delete team with active sessions");
        }

        await _unitOfWork.Teams.DeleteAsync(team);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<TeamMemberDetailDto>> GetTeamMembersAsync(Guid teamId)
    {
        var members = await _unitOfWork.TeamMembers.FindAsync(m => m.TeamId == teamId);
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
        var result = new List<TeamMemberDetailDto>();

        foreach (var member in members)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(member.UserId);
            if (user != null)
            {
                result.Add(new TeamMemberDetailDto
                {
                    Id = member.Id,
                    UserId = member.UserId,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    Role = user.Role,
                    JoinedAt = member.JoinedAt,
                    IsLeader = team?.LeaderId == member.UserId
                });
            }
        }

        return result.OrderBy(m => m.JoinedAt);
    }

    public async Task<TeamMemberDetailDto> AddMemberAsync(Guid teamId, AddTeamMemberDto dto, Guid addedById)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
        if (team == null)
        {
            throw new InvalidOperationException("Team not found");
        }

        // Check team size constraint
        var currentCount = await GetMemberCountAsync(teamId);
        if (currentCount >= team.MaxMembers)
        {
            throw new InvalidOperationException($"Team has reached maximum capacity ({team.MaxMembers} members)");
        }

        // Find user by email or userId
        User? user = null;
        if (!string.IsNullOrEmpty(dto.Email))
        {
            user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (user == null)
            {
                throw new InvalidOperationException($"User with email '{dto.Email}' not found");
            }
        }
        else if (dto.UserId.HasValue)
        {
            user = await _unitOfWork.Users.GetByIdAsync(dto.UserId.Value);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }
        }
        else
        {
            throw new InvalidOperationException("Either email or userId must be provided");
        }

        // Check if already a member
        var existing = await _unitOfWork.TeamMembers.FirstOrDefaultAsync(
            m => m.TeamId == teamId && m.UserId == user.Id);
        if (existing != null)
        {
            throw new InvalidOperationException("User is already a team member");
        }

        var member = new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            UserId = user.Id,
            JoinedAt = DateTime.UtcNow
        };

        await _unitOfWork.TeamMembers.AddAsync(member);
        await _unitOfWork.SaveChangesAsync();

        return new TeamMemberDetailDto
        {
            Id = member.Id,
            UserId = member.UserId,
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            Role = user.Role,
            JoinedAt = member.JoinedAt,
            IsLeader = team.LeaderId == member.UserId
        };
    }

    public async Task RemoveMemberAsync(Guid teamId, Guid userId, Guid removedById)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
        if (team == null)
        {
            throw new InvalidOperationException("Team not found");
        }

        // Cannot remove the leader
        if (team.LeaderId == userId)
        {
            throw new InvalidOperationException("Cannot remove the team leader");
        }

        // Check if member has active sessions
        var activeSessions = await _unitOfWork.BrainstormingSessions.FindAsync(
            s => s.TeamId == teamId && s.Status == SessionStatus.InProgress);
        if (activeSessions.Any())
        {
            throw new InvalidOperationException("Cannot remove member during active session");
        }

        var member = await _unitOfWork.TeamMembers.FirstOrDefaultAsync(
            m => m.TeamId == teamId && m.UserId == userId);
        if (member == null)
        {
            throw new InvalidOperationException("Member not found");
        }

        await _unitOfWork.TeamMembers.DeleteAsync(member);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> ValidateTeamSizeAsync(Guid teamId)
    {
        var count = await GetMemberCountAsync(teamId);
        return count >= 3 && count <= MaxTeamSize; // Minimum 3 for meaningful brainstorming
    }

    public async Task<bool> IsMemberAsync(Guid teamId, Guid userId)
    {
        return await _unitOfWork.TeamMembers.ExistsAsync(
            m => m.TeamId == teamId && m.UserId == userId);
    }

    public async Task<int> GetMemberCountAsync(Guid teamId)
    {
        return await _unitOfWork.TeamMembers.CountAsync(m => m.TeamId == teamId);
    }

    private async Task<TeamDetailDto> MapToDetailDto(Team team)
    {
        var evt = await _unitOfWork.Events.GetByIdAsync(team.EventId);
        var leader = team.LeaderId.HasValue ? await _unitOfWork.Users.GetByIdAsync(team.LeaderId.Value) : null;
        var memberCount = await GetMemberCountAsync(team.Id);
        var members = await GetTeamMembersAsync(team.Id);

        return new TeamDetailDto
        {
            Id = team.Id,
            EventId = team.EventId,
            EventName = evt?.Name ?? "Unknown",
            Name = team.Name,
            Description = team.Description,
            LeaderId = team.LeaderId,
            LeaderName = leader != null ? $"{leader.FirstName} {leader.LastName}" : null,
            MaxMembers = team.MaxMembers,
            CurrentMemberCount = memberCount,
            CreatedAt = team.CreatedAt,
            Members = members.ToList()
        };
    }
}
