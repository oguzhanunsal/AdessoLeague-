using AdessoLeague.Domain.Draws;

namespace AdessoLeague.Application.Abstractions.Persistence;

public interface IDrawRepository
{
    Task AddAsync(Draw draw, CancellationToken cancellationToken);
}
