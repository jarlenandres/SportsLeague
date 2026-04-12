namespace SportsLeague.Domain.Entities;

public class Team : AuditBase
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Stadium { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public DateTime FoundedDate { get; set; }

    // Navigation property for related players - Colección de jugadores en el equipo
    public ICollection<Player> Players { get; set; } = new List<Player>();

    public ICollection<TournamentTeam> TournamentTeams { get; set; } = new List<TournamentTeam>();
}