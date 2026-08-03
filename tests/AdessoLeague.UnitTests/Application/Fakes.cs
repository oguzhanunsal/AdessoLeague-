using AdessoLeague.Application.Abstractions.Persistence;

namespace AdessoLeague.UnitTests.Application;

internal sealed class FakeDrawRepository : IDrawRepository
{
    private readonly List<Draw> _stored = [];

    public IReadOnlyList<Draw> Added => _stored;

    public Task AddAsync(Draw draw, CancellationToken cancellationToken)
    {
        _stored.Add(draw);

        return Task.CompletedTask;
    }

    public Task<Draw?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_stored.Find(draw => draw.Id == id));

    public Task<IReadOnlyList<Draw>> GetPageAsync(int page, int size, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Draw>>(_stored.Skip((page - 1) * size).Take(size).ToList());

    public Task<int> CountAsync(CancellationToken cancellationToken) => Task.FromResult(_stored.Count);
}

internal sealed class FakeTeamRepository(IReadOnlyList<Team> pool) : ITeamRepository
{
    public int Calls { get; private set; }

    public Task<IReadOnlyList<Team>> GetPoolAsync(CancellationToken cancellationToken)
    {
        Calls++;

        return Task.FromResult(pool);
    }
}

internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;

        return Task.FromResult(0);
    }
}
