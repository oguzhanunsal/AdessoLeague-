using AdessoLeague.Application.Contracts;

namespace AdessoLeague.Application.Abstractions.Persistence;

public interface IDrawQueries
{
    Task<DrawResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedList<DrawSummaryResponse>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken);
}
