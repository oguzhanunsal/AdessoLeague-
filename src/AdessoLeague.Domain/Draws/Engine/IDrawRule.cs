using AdessoLeague.Domain.Leagues;

namespace AdessoLeague.Domain.Draws.Engine;

public interface IDrawRule
{
    bool IsSatisfiedBy(DrawGroup group, Team team);
}
