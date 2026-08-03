using AdessoLeague.Domain.Draws;

namespace AdessoLeague.Application.Abstractions.Persistence;

public interface IDrawRepository
{
    Task AddAsync(Draw draw, CancellationToken cancellationToken);

    Task<Draw?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <param name="page">1-based page number.</param>
    Task<IReadOnlyList<Draw>> GetPageAsync(int page, int size, CancellationToken cancellationToken);

    Task<int> CountAsync(CancellationToken cancellationToken);
}
