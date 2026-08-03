using AdessoLeague.Domain.Leagues;

namespace AdessoLeague.Domain.Draws.Engine;

public sealed class BacktrackingRoundRobinStrategy(
    IRandomProvider randomProvider,
    IReadOnlyCollection<IDrawRule> rules) : IDrawStrategy
{
    public Result<Draw> Execute(DrawRequest request)
    {
        var required = request.GroupCount.Value * request.GroupCount.TeamsPerGroup;

        if (request.Pool.Count != required)
        {
            return Result.Failure<Draw>(Error.Validation(
                "Draw.PoolSizeMismatch",
                $"A draw into {request.GroupCount.Value} groups needs exactly {required} teams, but the pool holds {request.Pool.Count}."));
        }

        if (request.Pool.Select(team => team.Id).Distinct().Count() != request.Pool.Count)
        {
            return Result.Failure<Draw>(Error.Validation(
                "Draw.DuplicateTeamInPool",
                "The pool contains the same team more than once."));
        }

        var draw = Draw.Create(
            request.DrawId,
            request.DrawnBy,
            request.GroupCount,
            randomProvider.Seed,
            request.CreatedAtUtc);

        var remaining = new List<Team>(request.Pool);

        return Fill(draw, remaining, flatIndex: 0, required)
            ? Result.Success(draw)
            : Result.Failure<Draw>(Error.Conflict(
                "Draw.NoValidAssignment",
                "No assignment satisfies the draw rules for this pool."));
    }

    private bool Fill(Draw draw, List<Team> remaining, int flatIndex, int required)
    {
        if (flatIndex == required)
        {
            return true;
        }

        var groupOrdinal = flatIndex % draw.GroupCount.Value;
        var group = draw.GroupAt(groupOrdinal);

        var candidates = remaining
            .Where(team => rules.All(rule => rule.IsSatisfiedBy(group, team)))
            .ToList();

        randomProvider.Shuffle(candidates);

        foreach (var candidate in candidates)
        {
            remaining.Remove(candidate);

            if (draw.PlaceTeam(groupOrdinal, candidate).IsFailure)
            {
                remaining.Add(candidate);
                continue;
            }

            if (Fill(draw, remaining, flatIndex + 1, required))
            {
                return true;
            }

            draw.RemoveLastPlacement();
            remaining.Add(candidate);
        }

        return false;
    }
}
