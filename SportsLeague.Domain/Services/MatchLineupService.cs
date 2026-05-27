using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services;

public class MatchLineupService : IMatchLineupService
{
    private readonly IMatchLineupRepository _matchLineupRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<MatchLineupService> _logger;

    public MatchLineupService(
        IMatchLineupRepository matchLineupRepository,
        IMatchRepository matchRepository,
        IPlayerRepository playerRepository,
        ILogger<MatchLineupService> logger)
    {
        _matchLineupRepository = matchLineupRepository;
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _logger = logger;
    }

    public async Task<MatchLineup> AddPlayerToLineupAsync(int matchId, MatchLineup lineup)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);

        if (match == null)
        {
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");
        }

        if (match.Status != MatchStatus.Scheduled)
        {
            throw new InvalidOperationException("Solo se pueden registrar alineaciones en partidos Scheduled");
        }

        var player = await _playerRepository.GetByIdAsync(lineup.PlayerId);

        if (player == null)
        {
            throw new KeyNotFoundException($"No se encontró el jugador con ID {lineup.PlayerId}");
        }

        if (player.TeamId != match.HomeTeamId && player.TeamId != match.AwayTeamId)
        {
            throw new InvalidOperationException("El jugador no pertenece a ninguno de los equipos del partido");
        }

        var alreadyExists = await _matchLineupRepository
            .ExistsByMatchAndPlayerAsync(matchId, lineup.PlayerId);

        if (alreadyExists)
        {
            throw new InvalidOperationException("El jugador ya está registrado en la alineación de este partido");
        }

        if (lineup.IsStarter)
        {
            var startersCount = await _matchLineupRepository
                .CountStartersByMatchAndTeamAsync(matchId, player.TeamId);

            if (startersCount >= 11)
            {
                throw new InvalidOperationException("El equipo ya tiene 11 titulares registrados en este partido");
            }
        }

        lineup.MatchId = matchId;
        lineup.CreatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Adding player {PlayerId} to match {MatchId} lineup",
            lineup.PlayerId,
            matchId);

        return await _matchLineupRepository.CreateAsync(lineup);
    }

    public async Task<IEnumerable<MatchLineup>> GetLineupByMatchAsync(int matchId)
    {
        var matchExists = await _matchRepository.ExistsAsync(matchId);

        if (!matchExists)
        {
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");
        }

        return await _matchLineupRepository.GetByMatchAsync(matchId);
    }

    public async Task<IEnumerable<MatchLineup>> GetLineupByMatchAndTeamAsync(int matchId, int teamId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);

        if (match == null)
        {
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");
        }

        if (teamId != match.HomeTeamId && teamId != match.AwayTeamId)
        {
            throw new InvalidOperationException("El equipo no pertenece a este partido");
        }

        return await _matchLineupRepository.GetByMatchAndTeamAsync(matchId, teamId);
    }

    public async Task DeletePlayerFromLineupAsync(int matchId, int lineupId)
    {
        var matchExists = await _matchRepository.ExistsAsync(matchId);

        if (!matchExists)
        {
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");
        }

        var lineup = await _matchLineupRepository.GetByIdAsync(lineupId);

        if (lineup == null || lineup.MatchId != matchId)
        {
            throw new KeyNotFoundException($"No se encontró la alineación con ID {lineupId}");
        }

        await _matchLineupRepository.DeleteAsync(lineupId);
    }
}