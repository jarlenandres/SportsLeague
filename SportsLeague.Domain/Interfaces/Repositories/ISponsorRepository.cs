using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;

namespace SportsLeague.Domain.Interfaces.Repositories;

public interface ISponsorRepository : IGenericRepository<Sponsor>
{
    Task<IEnumerable<Tournament>> GetBySponsorsAsync(SponsorCategory category);

    Task<Sponsor?> GetByNameAsync(string name);
}
