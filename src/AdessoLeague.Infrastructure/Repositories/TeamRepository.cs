using AdessoLeague.Application.Abstractions.Persistence;
using AdessoLeague.Infrastructure.Persistence;

namespace AdessoLeague.Infrastructure.Repositories;

internal sealed class TeamRepository(LeagueDbContext context) : ITeamRepository
{
    // Tracked on purpose: the teams are reused as-is inside a new draw, and untracked instances
    // would be treated as inserts.
    public async Task<IReadOnlyList<Team>> GetPoolAsync(CancellationToken cancellationToken) =>
        await context.Teams
            .OrderBy(team => team.CountryId)
            .ThenBy(team => team.Name)
            .ToListAsync(cancellationToken);
}
