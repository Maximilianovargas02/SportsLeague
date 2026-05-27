using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers;

[ApiController]
[Route("api/match/{matchId}/lineup")]
public class MatchLineupController : ControllerBase
{
    private readonly IMatchLineupService _matchLineupService;
    private readonly IMapper _mapper;

    public MatchLineupController(
        IMatchLineupService matchLineupService,
        IMapper mapper)
    {
        _matchLineupService = matchLineupService;
        _mapper = mapper;
    }

    // POST: api/match/{matchId}/lineup
    [HttpPost]
    public async Task<ActionResult<MatchLineupDto>> AddPlayerToLineup(
        int matchId,
        [FromBody] CreateMatchLineupDto dto)
    {
        try
        {
            var lineup = _mapper.Map<MatchLineup>(dto);

            var createdLineup = await _matchLineupService
                .AddPlayerToLineupAsync(matchId, lineup);

            var response = _mapper.Map<MatchLineupDto>(createdLineup);

            return CreatedAtAction(
                nameof(GetLineupByMatch),
                new { matchId },
                response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // GET: api/match/{matchId}/lineup
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MatchLineupDto>>> GetLineupByMatch(
        int matchId)
    {
        try
        {
            var lineups = await _matchLineupService
                .GetLineupByMatchAsync(matchId);

            var response = _mapper.Map<IEnumerable<MatchLineupDto>>(lineups);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // GET: api/match/{matchId}/lineup/team/{teamId}
    [HttpGet("team/{teamId}")]
    public async Task<ActionResult<IEnumerable<MatchLineupDto>>> GetLineupByMatchAndTeam(
        int matchId,
        int teamId)
    {
        try
        {
            var lineups = await _matchLineupService
                .GetLineupByMatchAndTeamAsync(matchId, teamId);

            var response = _mapper.Map<IEnumerable<MatchLineupDto>>(lineups);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // DELETE: api/match/{matchId}/lineup/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayerFromLineup(
        int matchId,
        int id)
    {
        try
        {
            await _matchLineupService
                .DeletePlayerFromLineupAsync(matchId, id);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }
}