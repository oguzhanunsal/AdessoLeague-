using AdessoLeague.Domain.Leagues;

namespace AdessoLeague.Application.Abstractions.Persistence;

public interface ITeamRepository
{
    /// <summary>Every team in the league, with its country, ready to be drawn.</summary>
    Task<IReadOnlyList<Team>> GetPoolAsync(CancellationToken cancellationToken);
}
