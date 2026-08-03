namespace AdessoLeague.Domain.Draws.Engine;

// Re-checks the strategy's own output so a strategy bug surfaces here instead of in the database.
public sealed class DrawEngine(IDrawStrategy strategy)
{
    public Result<Draw> Run(DrawRequest request)
    {
        var result = strategy.Execute(request);

        if (result.IsFailure)
        {
            return result;
        }

        var validation = Validate(result.Value, request);

        return validation.IsFailure
            ? Result.Failure<Draw>(validation.Error)
            : result;
    }

    private static Result Validate(Draw draw, DrawRequest request)
    {
        var expectedGroupCount = request.GroupCount.Value;
        var expectedGroupSize = request.GroupCount.TeamsPerGroup;
        var groups = draw.Groups.ToList();

        if (groups.Count != expectedGroupCount)
        {
            return Violation($"Expected {expectedGroupCount} groups but got {groups.Count}.");
        }

        if (groups.Exists(group => group.Teams.Count != expectedGroupSize))
        {
            return Violation($"Every group must hold exactly {expectedGroupSize} teams.");
        }

        var expectedNames = GroupName.Sequence(expectedGroupCount).Select(name => name.Value);
        if (!groups.Select(group => group.Name.Value).SequenceEqual(expectedNames))
        {
            return Violation("Group names must run from \"A\" without gaps.");
        }

        var placedTeamIds = groups.SelectMany(group => group.Teams).Select(team => team.TeamId).ToList();
        if (placedTeamIds.Distinct().Count() != placedTeamIds.Count)
        {
            return Violation("A team was placed more than once.");
        }

        if (!placedTeamIds.ToHashSet().SetEquals(request.Pool.Select(team => team.Id)))
        {
            return Violation("The placed teams do not match the pool.");
        }

        foreach (var group in groups)
        {
            var countryIds = group.Teams.Select(team => team.Team.CountryId).ToList();
            if (countryIds.Distinct().Count() != countryIds.Count)
            {
                return Violation($"Group {group.Name} holds two teams from the same country.");
            }

            var positions = group.Teams.Select(team => team.Position);
            if (!positions.SequenceEqual(Enumerable.Range(1, expectedGroupSize)))
            {
                return Violation($"Group {group.Name} was not filled in round-robin order.");
            }
        }

        return Result.Success();
    }

    private static Result Violation(string message) =>
        Result.Failure(Error.Conflict("Draw.InvariantViolated", message));
}
