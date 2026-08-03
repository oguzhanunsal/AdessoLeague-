using AdessoLeague.Domain.Leagues;

namespace AdessoLeague.Application.Abstractions.Persistence;

public interface ITeamRepository
{
    Task<IReadOnlyList<Team>> GetPoolAsync(CancellationToken cancellationToken);
}
