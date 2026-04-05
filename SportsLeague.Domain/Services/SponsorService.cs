using System.Net.Mail;
using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;
namespace SportsLeague.Domain.Services;
public class SponsorService : ISponsorService
{
    private readonly ISponsorRepository _sponsorRepository;
    private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
    private readonly IGenericRepository<Tournament> _tournamentRepository;
    private readonly ILogger<SponsorService> _logger;
    public SponsorService(
        ISponsorRepository sponsorRepository,
        ITournamentSponsorRepository tournamentSponsorRepository,
        IGenericRepository<Tournament> tournamentRepository,
        ILogger<SponsorService> logger)
    {
        _sponsorRepository = sponsorRepository;
        _tournamentSponsorRepository = tournamentSponsorRepository;
        _tournamentRepository = tournamentRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Sponsor>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all sponsors");
        return await _sponsorRepository.GetAllAsync();
    }

    public async Task<Sponsor?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving sponsor with ID: {SponsorId}", id);
        return await _sponsorRepository.GetByIdAsync(id);
    }
    public async Task<Sponsor> CreateAsync(Sponsor sponsor)
    {
        if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name))
        {
            _logger.LogWarning("Sponsor with name '{SponsorName}' already exists", sponsor.Name);
            throw new InvalidOperationException($"A sponsor with the name '{sponsor.Name}' already exists.");
        }
        if (!IsValidEmail(sponsor.ContactEmail))
        {
            _logger.LogWarning("Invalid email format for sponsor '{SponsorName}'", sponsor.Name);
            throw new InvalidOperationException("ContactEmail must be a valid email address.");
        }
        sponsor.CreatedAt = DateTime.UtcNow;
        _logger.LogInformation("Creating sponsor: {SponsorName}", sponsor.Name);
        return await _sponsorRepository.CreateAsync(sponsor);
    }
    public async Task UpdateAsync(int id, Sponsor sponsor)
    {
        var existingSponsor = await _sponsorRepository.GetByIdAsync(id);
        if (existingSponsor == null)
        {
            _logger.LogWarning("Sponsor with ID {SponsorId} not found for update", id);
            throw new KeyNotFoundException($"Sponsor with ID {id} was not found.");
        }
        if (!string.Equals(existingSponsor.Name, sponsor.Name, StringComparison.OrdinalIgnoreCase))
        {
            if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name))
            {
                _logger.LogWarning("Sponsor with name '{SponsorName}' already exists", sponsor.Name);
                throw new InvalidOperationException($"A sponsor with the name '{sponsor.Name}' already exists.");
            }
        }
        if (!IsValidEmail(sponsor.ContactEmail))
        {
            _logger.LogWarning("Invalid email format for sponsor '{SponsorName}'", sponsor.Name);
            throw new InvalidOperationException("ContactEmail must be a valid email address.");
        }
        existingSponsor.Name = sponsor.Name;
        existingSponsor.ContactEmail = sponsor.ContactEmail;
        existingSponsor.Phone = sponsor.Phone;
        existingSponsor.WebsiteUrl = sponsor.WebsiteUrl;
        existingSponsor.Category = sponsor.Category;
        existingSponsor.UpdatedAt = DateTime.UtcNow;
        _logger.LogInformation("Updating sponsor with ID: {SponsorId}", id);
        await _sponsorRepository.UpdateAsync(existingSponsor);
    }
    public async Task DeleteAsync(int id)
    {
        var exists = await _sponsorRepository.ExistsAsync(id);
        if (!exists)
        {
            _logger.LogWarning("Sponsor with ID {SponsorId} not found for deletion", id);
            throw new KeyNotFoundException($"Sponsor with ID {id} was not found.");
        }
        _logger.LogInformation("Deleting sponsor with ID: {SponsorId}", id);
        await _sponsorRepository.DeleteAsync(id);
    }
    public async Task<IEnumerable<TournamentSponsor>> GetTournamentsBySponsorIdAsync(int sponsorId)
    {
        var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);
        if (!sponsorExists)
        {
            _logger.LogWarning("Sponsor with ID {SponsorId} not found for tournament listing", sponsorId);
            throw new KeyNotFoundException($"Sponsor with ID {sponsorId} was not found.");
        }

        _logger.LogInformation("Retrieving tournaments for sponsor ID: {SponsorId}", sponsorId);
        return await _tournamentSponsorRepository.GetBySponsorIdAsync(sponsorId);
    }
    public async Task<TournamentSponsor> LinkTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount)
    {
        var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);
        if (!sponsorExists)
        {
            _logger.LogWarning("Sponsor with ID {SponsorId} not found for link", sponsorId);
            throw new KeyNotFoundException($"Sponsor with ID {sponsorId} was not found.");
        }
        var tournamentExists = await _tournamentRepository.ExistsAsync(tournamentId);
        if (!tournamentExists)
        {
            _logger.LogWarning("Tournament with ID {TournamentId} not found for link", tournamentId);
            throw new KeyNotFoundException($"Tournament with ID {tournamentId} was not found.");
        }
        var alreadyLinked = await _tournamentSponsorRepository.ExistsAsync(tournamentId, sponsorId);
        if (alreadyLinked)
        {
            _logger.LogWarning(
                "Sponsor ID {SponsorId} is already linked to tournament ID {TournamentId}",
                sponsorId,
                tournamentId);
            throw new InvalidOperationException("This sponsor is already linked to this tournament.");
        }
        if (contractAmount <= 0)
        {
            _logger.LogWarning(
                "Invalid contract amount {ContractAmount} for sponsor ID {SponsorId} and tournament ID {TournamentId}",
                contractAmount,
                sponsorId,
                tournamentId);

            throw new InvalidOperationException("ContractAmount must be greater than 0.");
        }
        var tournamentSponsor = new TournamentSponsor
        {
            SponsorId = sponsorId,
            TournamentId = tournamentId,
            ContractAmount = contractAmount,
            JoinedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Linking sponsor ID {SponsorId} to tournament ID {TournamentId}",
            sponsorId,
            tournamentId);

        return await _tournamentSponsorRepository.CreateAsync(tournamentSponsor);
    }
    public async Task UnlinkTournamentAsync(int sponsorId, int tournamentId)
    {
        var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);
        if (!sponsorExists)
        {
            _logger.LogWarning("Sponsor with ID {SponsorId} not found for unlink", sponsorId);
            throw new KeyNotFoundException($"Sponsor with ID {sponsorId} was not found.");
        }
        var tournamentExists = await _tournamentRepository.ExistsAsync(tournamentId);
        if (!tournamentExists)
        {
            _logger.LogWarning("Tournament with ID {TournamentId} not found for unlink", tournamentId);
            throw new KeyNotFoundException($"Tournament with ID {tournamentId} was not found.");
        }

        var links = await _tournamentSponsorRepository.GetBySponsorIdAsync(sponsorId);
        var tournamentSponsor = links.FirstOrDefault(ts => ts.TournamentId == tournamentId);

        if (tournamentSponsor == null)
        {
            _logger.LogWarning(
                "Link between sponsor ID {SponsorId} and tournament ID {TournamentId} was not found",
                sponsorId,
                tournamentId);

            throw new KeyNotFoundException("The sponsor is not linked to this tournament.");
        }

        _logger.LogInformation(
            "Unlinking sponsor ID {SponsorId} from tournament ID {TournamentId}",
            sponsorId,
            tournamentId);

        await _tournamentSponsorRepository.DeleteAsync(tournamentSponsor.Id);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var mailAddress = new MailAddress(email);
            return mailAddress.Address == email;
        }
        catch
        {
            return false;
        }
    }
}