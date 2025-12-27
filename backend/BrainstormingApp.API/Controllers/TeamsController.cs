using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BrainstormingApp.Application.Common;
using BrainstormingApp.Application.Common.Exceptions;
using BrainstormingApp.Application.DTOs.Team;
using BrainstormingApp.Application.Interfaces;

namespace BrainstormingApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    /// <summary>
    /// Get all teams for an event
    /// </summary>
    [HttpGet("event/{eventId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TeamDetailDto>>>> GetByEvent(Guid eventId)
    {
        var teams = await _teamService.GetTeamsByEventAsync(eventId);
        return Ok(ApiResponse<IEnumerable<TeamDetailDto>>.SuccessResponse(teams));
    }

    /// <summary>
    /// Get a specific team by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TeamDetailDto>>> GetById(Guid id)
    {
        var team = await _teamService.GetTeamByIdAsync(id);
        if (team == null)
        {
            throw new NotFoundException("Team", id);
        }
        return Ok(ApiResponse<TeamDetailDto>.SuccessResponse(team));
    }

    /// <summary>
    /// Create a new team
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TeamDetailDto>>> Create([FromBody] CreateTeamDto dto)
    {
        var userId = GetCurrentUserId();
        var team = await _teamService.CreateTeamAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = team.Id },
            ApiResponse<TeamDetailDto>.SuccessResponse(team, "Team created successfully"));
    }

    /// <summary>
    /// Update an existing team
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<TeamDetailDto>>> Update(Guid id, [FromBody] UpdateTeamDto dto)
    {
        var userId = GetCurrentUserId();
        var team = await _teamService.UpdateTeamAsync(id, dto, userId);
        return Ok(ApiResponse<TeamDetailDto>.SuccessResponse(team, "Team updated successfully"));
    }

    /// <summary>
    /// Delete a team
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        await _teamService.DeleteTeamAsync(id, userId);
        return Ok(ApiResponse.SuccessResult("Team deleted successfully"));
    }

    /// <summary>
    /// Get team members
    /// </summary>
    [HttpGet("{id}/members")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TeamMemberDetailDto>>>> GetMembers(Guid id)
    {
        var members = await _teamService.GetTeamMembersAsync(id);
        return Ok(ApiResponse<IEnumerable<TeamMemberDetailDto>>.SuccessResponse(members));
    }

    /// <summary>
    /// Add a member to the team
    /// </summary>
    [HttpPost("{id}/members")]
    public async Task<ActionResult<ApiResponse<TeamMemberDetailDto>>> AddMember(Guid id, [FromBody] AddTeamMemberDto dto)
    {
        var userId = GetCurrentUserId();
        var member = await _teamService.AddMemberAsync(id, dto, userId);
        return CreatedAtAction(nameof(GetMembers), new { id },
            ApiResponse<TeamMemberDetailDto>.SuccessResponse(member, "Member added successfully"));
    }

    /// <summary>
    /// Remove a member from the team
    /// </summary>
    [HttpDelete("{teamId}/members/{memberId}")]
    public async Task<ActionResult<ApiResponse>> RemoveMember(Guid teamId, Guid memberId)
    {
        var currentUserId = GetCurrentUserId();
        await _teamService.RemoveMemberAsync(teamId, memberId, currentUserId);
        return Ok(ApiResponse.SuccessResult("Member removed successfully"));
    }

    /// <summary>
    /// Check if team has valid size for brainstorming (3-6 members)
    /// </summary>
    [HttpGet("{id}/validate-size")]
    public async Task<ActionResult<ApiResponse<object>>> ValidateSize(Guid id)
    {
        var isValid = await _teamService.ValidateTeamSizeAsync(id);
        var memberCount = await _teamService.GetMemberCountAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(new { isValid, memberCount, minRequired = 3, maxAllowed = 6 }));
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
