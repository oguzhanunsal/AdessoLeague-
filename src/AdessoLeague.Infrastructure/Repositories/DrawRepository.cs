using AdessoLeague.Application.Abstractions.Persistence;
using AdessoLeague.Infrastructure.Persistence;

namespace AdessoLeague.Infrastructure.Repositories;

internal sealed class DrawRepository(LeagueDbContext context) : IDrawRepository
{
    public async Task AddAsync(Draw draw, CancellationToken cancellationToken) =>
        await context.Draws.AddAsync(draw, cancellationToken);
}
