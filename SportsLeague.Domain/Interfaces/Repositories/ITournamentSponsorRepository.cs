using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.Domain.Interfaces.Repositories;

public interface ITournamentSponsorRepository : IGenericRepository<TournamentSponsor>
{
    Task<bool> ExistsAsync(int tournamentId, int sponsorId);

    Task<List<TournamentSponsor>> GetBySponsorIdAsync(int sponsorId);

    Task<List<TournamentSponsor>> GetByTournamentIdAsync(int tournamentId);
}