using AdessoLeague.Application.Abstractions.Persistence;
using AdessoLeague.Infrastructure.Persistence;

namespace AdessoLeague.Infrastructure.Repositories;

internal sealed class UnitOfWork(LeagueDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        await context.SaveChangesAsync(cancellationToken);
}
