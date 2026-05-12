using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services;

public class MatchService : IMatchService
{
    private readonly IMatchRepository _matchRepository;
    private readonly ITournamentRepository _tournamentRepository;
    private readonly ITournamentTeamRepository _tournamentTeamRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IRefereeRepository _refereeRepository;
    private readonly ILogger<MatchService> _logger;

    public MatchService(
        IMatchRepository matchRepository,
        ITournamentRepository tournamentRepository,
        ITournamentTeamRepository tournamentTeamRepository,
        ITeamRepository teamRepository,
        IRefereeRepository refereeRepository,
        ILogger<MatchService> logger)
    {
        _matchRepository = matchRepository;
        _tournamentRepository = tournamentRepository;
        _tournamentTeamRepository = tournamentTeamRepository;
        _teamRepository = teamRepository;
        _refereeRepository = refereeRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Match>> GetAllByTournamentAsync(int tournamentId)
    {
        var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
        if (tournament == null)
        {
            throw new KeyNotFoundException($"No se encontró el torneo con ID {tournamentId}.");
        }
        return await _matchRepository.GetByTournamentWithDetailsAsync(tournamentId);
    }

    public async Task<Match> CreateAsync(Match match)
    {
        // Validar que el torneo exista y está en proceso
        var tournament = await _tournamentRepository.GetByIdAsync(match.TournamentId);
        if (tournament == null)
        {
            throw new KeyNotFoundException($"No se encontró el torneo con ID {match.TournamentId}.");
        }

        if (tournament.Status != TournamentStatus.InProgress)
        {
            throw new InvalidOperationException("Soo se pueden programar partidos en torneos con estado InProgress.");
        }

        // Validar que los equipos son diferentes
        if (match.HomeTeamId == match.AwayTeamId)
        {
            throw new InvalidOperationException("El equipo local y el equipo visitante deben ser diferentes.");
        }

        // Validar que ambos equipos existan
        var homeTeamExists = await _teamRepository.ExistsAsync(match.HomeTeamId);
        if(!homeTeamExists)
        {
            throw new KeyNotFoundException($"No se encontró el equipo local con ID {match.HomeTeamId}");
        }

        var awayTeamExists = await _teamRepository.ExistsAsync(match.AwayTeamId);
        if (!awayTeamExists)
        {
            throw new KeyNotFoundException($"No se encontró el equipo visitante con ID {match.AwayTeamId}");
        }

        // Validar que ambos equipos estén inscritos en el torneo
        var homeEnrolled = await _tournamentTeamRepository.GetByTournamentAndTeamAsync(match.TournamentId, match.HomeTeamId);
        if (homeEnrolled == null)
        {
            throw new InvalidOperationException("El equipo local no está inscrito en el torneo.");
        }

        var awayEnrolled = await _tournamentTeamRepository.GetByTournamentAndTeamAsync(match.TournamentId, match.AwayTeamId);
        if (awayEnrolled == null)
        {
            throw new InvalidOperationException("El equipo visitante no está inscrito en el torneo.");
        }

        // Validar que el árbitro exista
        var refereeExists = await _refereeRepository.ExistsAsync(match.RefereeId);
        if (!refereeExists)
        {
            throw new KeyNotFoundException($"No se encontró el árbitro con ID {match.RefereeId}");
        }

        match.Status = MatchStatus.Scheduled;

        _logger.LogInformation("Creating match: Team {Home} vs Team {Away} in Tournament {Tournament}", match.HomeTeamId, match.AwayTeamId, match.TournamentId);
        return await _matchRepository.CreateAsync(match);
    }

    public Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<Match?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving match with ID {MatchId}", id);
        return await _matchRepository.GetByWithDetailAsync(id);
    }

    public Task UpdateAsync(int id, Match match)
    {
        throw new NotImplementedException();
    }

    public Task UpdateStatusAsync(int id, MatchStatus newStatus)
    {
        throw new NotImplementedException();
    }
}
