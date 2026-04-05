using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SponsorController : ControllerBase
{
    private readonly ISponsorService _sponsorService;
    private readonly IMapper _mapper;
    private readonly ILogger<SponsorController> _logger;

    public SponsorController(
        ISponsorService sponsorService,
        IMapper mapper,
        ILogger<SponsorController> logger)
    {
        _sponsorService = sponsorService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SponsorResponseDTO>>> GetAll()
    {
        var sponsors = await _sponsorService.GetAllAsync();
        var sponsorsDto = _mapper.Map<IEnumerable<SponsorResponseDTO>>(sponsors);

        return Ok(sponsorsDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SponsorResponseDTO>> GetById(int id)
    {
        var sponsor = await _sponsorService.GetByIdAsync(id);

        if (sponsor == null)
        {
            return NotFound(new { message = $"Sponsor with ID {id} was not found." });
        }

        var sponsorDto = _mapper.Map<SponsorResponseDTO>(sponsor);

        return Ok(sponsorDto);
    }

    [HttpPost]
    public async Task<ActionResult<SponsorResponseDTO>> Create(SponsorRequestDTO dto)
    {
        try
        {
            var sponsor = _mapper.Map<Sponsor>(dto);
            var createdSponsor = await _sponsorService.CreateAsync(sponsor);
            var responseDto = _mapper.Map<SponsorResponseDTO>(createdSponsor);

            return CreatedAtAction(nameof(GetById), new { id = responseDto.Id }, responseDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business validation error while creating sponsor");
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, SponsorRequestDTO dto)
    {
        try
        {
            var sponsor = _mapper.Map<Sponsor>(dto);
            await _sponsorService.UpdateAsync(id, sponsor);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Sponsor not found while updating");
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business validation error while updating sponsor");
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _sponsorService.DeleteAsync(id);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Sponsor not found while deleting");
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/tournaments")]
    public async Task<ActionResult<IEnumerable<TournamentSponsorResponseDTO>>> GetTournaments(int id)
    {
        try
        {
            var tournamentSponsors = await _sponsorService.GetTournamentsBySponsorIdAsync(id);
            var response = _mapper.Map<IEnumerable<TournamentSponsorResponseDTO>>(tournamentSponsors);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Sponsor not found while listing tournaments");
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/tournaments")]
    public async Task<ActionResult<TournamentSponsorResponseDTO>> LinkTournament(int id, TournamentSponsorRequestDTO dto)
    {
        try
        {
            var tournamentSponsor = await _sponsorService.LinkTournamentAsync(id, dto.TournamentId, dto.ContractAmount);
            var response = _mapper.Map<TournamentSponsorResponseDTO>(tournamentSponsor);

            return CreatedAtAction(nameof(GetTournaments), new { id }, response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Entity not found while linking sponsor to tournament");
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business validation error while linking sponsor to tournament");
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}/tournaments/{tournamentId}")]
    public async Task<IActionResult> UnlinkTournament(int id, int tournamentId)
    {
        try
        {
            await _sponsorService.UnlinkTournamentAsync(id, tournamentId);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Entity or relationship not found while unlinking sponsor from tournament");
            return NotFound(new { message = ex.Message });
        }
    }
}