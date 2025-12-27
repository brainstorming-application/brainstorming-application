using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    /// Get teams for the current user
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<TeamDetailDto>>> GetMyTeams()
    {
        try
        {
            var userId = GetCurrentUserId();
            var teams = await _teamService.GetMyTeamsAsync(userId);
            return Ok(teams);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all teams for an event
    /// </summary>
    [HttpGet("event/{eventId}")]
    public async Task<ActionResult<IEnumerable<TeamDetailDto>>> GetByEvent(Guid eventId)
    {
        try
        {
            var teams = await _teamService.GetTeamsByEventAsync(eventId);
            return Ok(teams);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific team by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TeamDetailDto>> GetById(Guid id)
    {
        try
        {
            var team = await _teamService.GetTeamByIdAsync(id);
            if (team == null)
            {
                return NotFound(new { message = "Team not found" });
            }
            return Ok(team);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new team
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TeamDetailDto>> Create([FromBody] CreateTeamDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var team = await _teamService.CreateTeamAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = team.Id }, team);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing team
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TeamDetailDto>> Update(Guid id, [FromBody] UpdateTeamDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var team = await _teamService.UpdateTeamAsync(id, dto, userId);
            return Ok(team);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a team
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _teamService.DeleteTeamAsync(id, userId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get team members
    /// </summary>
    [HttpGet("{id}/members")]
    public async Task<ActionResult<IEnumerable<TeamMemberDetailDto>>> GetMembers(Guid id)
    {
        try
        {
            var members = await _teamService.GetTeamMembersAsync(id);
            return Ok(members);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Add a member to the team
    /// </summary>
    [HttpPost("{id}/members")]
    public async Task<ActionResult<TeamMemberDetailDto>> AddMember(Guid id, [FromBody] AddTeamMemberDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var member = await _teamService.AddMemberAsync(id, dto, userId);
            return CreatedAtAction(nameof(GetMembers), new { id }, member);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove a member from the team
    /// </summary>
    [HttpDelete("{teamId}/members/{userId}")]
    public async Task<ActionResult> RemoveMember(Guid teamId, Guid userId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            await _teamService.RemoveMemberAsync(teamId, userId, currentUserId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Check if team has valid size for brainstorming (3-6 members)
    /// </summary>
    [HttpGet("{id}/validate-size")]
    public async Task<ActionResult<bool>> ValidateSize(Guid id)
    {
        try
        {
            var isValid = await _teamService.ValidateTeamSizeAsync(id);
            var memberCount = await _teamService.GetMemberCountAsync(id);
            return Ok(new { isValid, memberCount, minRequired = 3, maxAllowed = 6 });
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
