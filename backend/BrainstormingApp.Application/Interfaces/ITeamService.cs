using BrainstormingApp.Application.DTOs.Team;

namespace BrainstormingApp.Application.Interfaces;

public interface ITeamService
{
    Task<IEnumerable<TeamDetailDto>> GetTeamsByEventAsync(Guid eventId);
    Task<IEnumerable<TeamDetailDto>> GetMyTeamsAsync(Guid userId);
    Task<TeamDetailDto?> GetTeamByIdAsync(Guid teamId);
    Task<TeamDetailDto> CreateTeamAsync(CreateTeamDto dto, Guid userId);
    Task<TeamDetailDto> UpdateTeamAsync(Guid teamId, UpdateTeamDto dto, Guid userId);
    Task DeleteTeamAsync(Guid teamId, Guid userId);

    // Member management
    Task<IEnumerable<TeamMemberDetailDto>> GetTeamMembersAsync(Guid teamId);
    Task<TeamMemberDetailDto> AddMemberAsync(Guid teamId, AddTeamMemberDto dto, Guid addedById);
    Task RemoveMemberAsync(Guid teamId, Guid userId, Guid removedById);
    Task<bool> ValidateTeamSizeAsync(Guid teamId);
    Task<bool> IsMemberAsync(Guid teamId, Guid userId);
    Task<int> GetMemberCountAsync(Guid teamId);
}
