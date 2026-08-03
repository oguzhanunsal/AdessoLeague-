using AdessoLeague.Application.Abstractions.Persistence;
using AdessoLeague.Infrastructure.Persistence;

namespace AdessoLeague.Infrastructure.Repositories;

internal sealed class DrawRepository(LeagueDbContext context) : IDrawRepository
{
    public async Task AddAsync(Draw draw, CancellationToken cancellationToken) =>
        await context.Draws.AddAsync(draw, cancellationToken);

    public async Task<Draw?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await Complete(context.Draws.AsNoTracking())
            .FirstOrDefaultAsync(draw => draw.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Draw>> GetPageAsync(int page, int size, CancellationToken cancellationToken) =>
        await Complete(context.Draws.AsNoTracking())
            .OrderByDescending(draw => draw.CreatedAtUtc)
            .ThenByDescending(draw => draw.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

    public async Task<int> CountAsync(CancellationToken cancellationToken) =>
        await context.Draws.CountAsync(cancellationToken);

    // Ordering lives in the query because the draw order is what the stored ordinals and positions mean.
    private static IQueryable<Draw> Complete(IQueryable<Draw> draws) => draws
        .Include(draw => draw.Groups.OrderBy(group => group.Ordinal))
            .ThenInclude(group => group.Teams.OrderBy(placement => placement.Position))
                .ThenInclude(placement => placement.Team)
        .AsSplitQuery();
}
