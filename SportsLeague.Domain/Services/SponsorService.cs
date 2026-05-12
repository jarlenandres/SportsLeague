using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;
using System.Net.Mail;

namespace SportsLeague.Domain.Services;

public class SponsorService : ISponsorService
{
    private readonly ISponsorRepository _sponsorRepository;
    private readonly ITournamentRepository _tournamentRepository;
    private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
    private readonly ILogger<SponsorService> _logger;

    public SponsorService(
        ISponsorRepository sponsorRepository,
        ITournamentRepository tournamentRepository,
        ITournamentSponsorRepository tournamentSponsorRepository,
        ILogger<SponsorService> logger)
    {
        _sponsorRepository = sponsorRepository;
        _tournamentRepository = tournamentRepository;
        _tournamentSponsorRepository = tournamentSponsorRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Sponsor>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all sponsor");
        return await _sponsorRepository.GetAllAsync();
    }

    public async Task<Sponsor?> GetByIdAsync(int  id)
    {
        _logger.LogInformation("Retrieving sponsor with ID {sponsorId}", id);
        var sponsor = await _sponsorRepository.GetByIdAsync(id);

        if (sponsor == null) 
        {
            _logger.LogWarning("Sponsor with ID {sponsorId} not found", id);
        }
        return sponsor;
    }
    

    public async Task<Sponsor> CreateAsync(Sponsor sponsor)
    {
        // Validación de negocio
        if (sponsor == null)
        {
            throw new ArgumentNullException(nameof(sponsor));
        }

        if (string.IsNullOrWhiteSpace(sponsor.Name))
        {
            throw new ArgumentException("El nombre del patrocinador es obligatorio.", nameof(sponsor.Name));
        }

        if (string.IsNullOrWhiteSpace(sponsor.ContactEmail))
        {
            throw new ArgumentException("El correo es obligatorio.", nameof(sponsor.ContactEmail));
        }

        var normalizedName = sponsor.Name.Trim();
        var normalizedEmail = sponsor.ContactEmail.Trim();
        var normalizedPhono = string.IsNullOrWhiteSpace(sponsor.Phone) ? null : sponsor.Phone.Trim();
        var normalizedWebsite = string.IsNullOrWhiteSpace(sponsor.WebsiteUrl) ? null : sponsor.WebsiteUrl.Trim();

        ValidateEmail(normalizedEmail);
        
        var existingSponsor = await _sponsorRepository.GetByNameAsync(normalizedName);
        if (existingSponsor != null)
        {
            _logger.LogWarning("Sponsor with name {SponsorName} already existing", normalizedName);
            throw new InvalidOperationException($"Ya existe un patrocinador con ese nombre '{sponsor.Name}' ");
        }

        sponsor.Name = normalizedName;
        sponsor.ContactEmail = normalizedEmail;
        sponsor.Phone = normalizedPhono;
        sponsor.WebsiteUrl = normalizedWebsite;

        _logger.LogInformation("Creating sponsor: {SponsorName}", sponsor.Name);
        return await _sponsorRepository.CreateAsync(sponsor);
    }

    public async Task UpdateAsync(int id, Sponsor sponsor)
    {
        // Validaciones de negocio
        var existingSponsor = await _sponsorRepository.GetByIdAsync(id);

        if (existingSponsor == null)
        {
            throw new KeyNotFoundException($"No se encontro el patrocinador con ID {id}");
        }

        if (string.IsNullOrWhiteSpace(sponsor.Name))
        {
            throw new ArgumentException("El nombre del patrocinador es obligatorio", nameof(sponsor.Name));
        }

        if (string.IsNullOrWhiteSpace(sponsor.ContactEmail))
        {
            throw new ArgumentException("El correo es obligatorio", nameof (sponsor.ContactEmail));
        }

        // Actualizar datos del sponsor
        existingSponsor.Name = sponsor.Name;
        existingSponsor.ContactEmail = sponsor.ContactEmail;
        existingSponsor.Phone = sponsor.Phone;
        existingSponsor.WebsiteUrl= sponsor.WebsiteUrl;

        _logger.LogInformation("Updating sponsor with ID {SponsorId}.", id);
        await _sponsorRepository.UpdateAsync(existingSponsor);
    }

    public async Task DeleteAsync(int id)
    {
        var exists = await _sponsorRepository.ExistsAsync(id);
        if (!exists)
        {
            throw new KeyNotFoundException($"No se encontro el jugador con ID {id}");
        }

        _logger.LogInformation("Deleting sponsor with ID {SponsorId}", id);
        await _sponsorRepository.DeleteAsync(id);
    }

    public Task<IEnumerable<TournamentSponsor>> GetSponsorByTournamentAsync(int sponsorId)
    {
        var sponsor = _sponsorRepository.ExistsAsync(sponsorId);
        if (sponsor == null)
        {
            throw new KeyNotFoundException($"No se encontro el patrocinador con ID {sponsorId}");
        }
        return _tournamentSponsorRepository.GetBySponsorAsync(sponsorId);
    }

    private static void ValidateEmail(string contactEmail)
    {
        try
        {
            _ = new MailAddress(contactEmail);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("El correo debe tener un formato valido.");
        }
    }

    public async Task<TournamentSponsor> LinkTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount)
    {
        var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
        if (sponsor == null)
        {
            throw new KeyNotFoundException($"No se encontro el patrocinador con ID {sponsorId}.");
        }

        var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
        if (tournament == null)
        {
            throw new KeyNotFoundException($"No se encontro el torneo con ID {tournamentId}.");
        }

        var existingLink = await _tournamentSponsorRepository.GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
        if (existingLink != null)
        {
            throw new InvalidOperationException("El patrocinador ya está vinculado al torneo");
        }

        if (contractAmount <= 0)
        {
            throw new InvalidOperationException("El valor del contrato debe ser mayor a 0");
        }

        var tournamentSponsor = new TournamentSponsor
        {
            TournamentId = tournamentId,
            SponsorId = sponsorId,
            ContractAmount = contractAmount,
            JoinedAt = DateTime.UtcNow
        };

        _logger.LogInformation("<linking sporsor {SponsorId} to tournament {TournamentId}", sponsorId, tournamentId);

        await _tournamentSponsorRepository.CreateAsync(tournamentSponsor);
        return await _tournamentSponsorRepository.GetByTournamentAndSponsorAsync(tournamentId, sponsorId) ?? tournamentSponsor;
    }

    public async Task<IEnumerable<TournamentSponsor>> GetTournamentsBySponsorAsync(int sponsorId)
    {
        var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
        if (sponsor == null)
        {
            throw new KeyNotFoundException($"No se encontro el patrocinador con ID {sponsorId}.");
        }

        return await _tournamentSponsorRepository.GetBySponsorAsync(sponsorId);

    }

    public async Task UnlinkTournamentAsync(int sponsorId, int tournamentId)
    {
        var existingLink = await _tournamentSponsorRepository.GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
        if (existingLink == null)
        {
            throw new KeyNotFoundException($"No se encontro la vinculación del patrocinador {sponsorId} con el torneo {tournamentId}.");
        }

        _logger.LogInformation("Unlinking sponsor {SponsorId} from tournament {tournamentId}", sponsorId, tournamentId);

        await _tournamentSponsorRepository.DeleteAsync(existingLink.Id);
    }
}
