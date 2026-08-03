using AdessoLeague.Application.Abstractions.Persistence;
using AdessoLeague.Application.Contracts;
using AdessoLeague.Application.Mapping;

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
}

internal sealed class FakeDrawQueries : IDrawQueries
{
    private readonly List<Draw> _stored = [];

    public int? LastPage { get; private set; }

    public int? LastPageSize { get; private set; }

    public void Store(Draw draw) => _stored.Add(draw);

    public Task<DrawResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_stored.Find(draw => draw.Id == id)?.ToResponse());

    public Task<PagedList<DrawSummaryResponse>> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        LastPage = page;
        LastPageSize = pageSize;

        var items = _stored
            .OrderByDescending(draw => draw.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(draw => draw.ToSummaryResponse())
            .ToList();

        return Task.FromResult(new PagedList<DrawSummaryResponse>(items, page, pageSize, _stored.Count));
    }
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
