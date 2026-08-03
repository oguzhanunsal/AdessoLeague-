using AdessoLeague.Domain.Leagues;

namespace AdessoLeague.Domain.Draws;

// The one-team-per-country constraint is not enforced here: a placement records only a team id, so
// the aggregate cannot see countries. It is applied while selecting a candidate, by the draw rules.
public sealed class Draw
{
    private readonly List<DrawGroup> _groups = [];

    private Draw()
    {
        DrawnBy = null!;
        GroupCount = null!;
    }

    private Draw(Guid id, DrawnBy drawnBy, GroupCount groupCount, int seed, DateTime createdAtUtc)
    {
        Id = id;
        DrawnBy = drawnBy;
        GroupCount = groupCount;
        Seed = seed;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public DrawnBy DrawnBy { get; private set; }

    public GroupCount GroupCount { get; private set; }

    public int Seed { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<DrawGroup> Groups => _groups.AsReadOnly();

    public int PlacedTeamCount => _groups.Sum(group => group.Teams.Count);

    public bool IsComplete => PlacedTeamCount == GroupCount.Value * GroupCount.TeamsPerGroup;

    public static Draw Create(Guid id, DrawnBy drawnBy, GroupCount groupCount, int seed, DateTime createdAtUtc)
    {
        var draw = new Draw(id, drawnBy, groupCount, seed, createdAtUtc);
        var names = GroupName.Sequence(groupCount.Value);

        for (var ordinal = 0; ordinal < names.Count; ordinal++)
        {
            draw._groups.Add(DrawGroup.Create(Guid.NewGuid(), id, names[ordinal], ordinal));
        }

        return draw;
    }

    // Placements must arrive round-robin: group A slot 1, group B slot 1, and so on.
    public Result PlaceTeam(int groupOrdinal, Team team)
    {
        if (groupOrdinal < 0 || groupOrdinal >= _groups.Count)
        {
            return Result.Failure(Error.Validation(
                "Draw.UnknownGroup",
                $"Group ordinal {groupOrdinal} is outside this draw, which has {_groups.Count} groups."));
        }

        var expectedOrdinal = PlacedTeamCount % _groups.Count;
        if (groupOrdinal != expectedOrdinal)
        {
            return Result.Failure(Error.Validation(
                "Draw.OutOfOrderPlacement",
                $"Teams are placed round-robin; the next group is {expectedOrdinal}, not {groupOrdinal}."));
        }

        if (_groups.Exists(group => group.Contains(team.Id)))
        {
            return Result.Failure(Error.Validation(
                "Draw.DuplicateTeam",
                $"Team {team.Id} has already been placed in this draw."));
        }

        var target = _groups[groupOrdinal];
        if (target.Teams.Count >= GroupCount.TeamsPerGroup)
        {
            return Result.Failure(Error.Validation(
                "Draw.GroupFull",
                $"Group {target.Name} already holds {GroupCount.TeamsPerGroup} teams."));
        }

        target.Place(team);

        return Result.Success();
    }

    internal DrawGroup GroupAt(int ordinal) => _groups[ordinal];

    // Backtracking undo; internal so the aggregate stays append-only for every other caller.
    internal void RemoveLastPlacement()
    {
        if (PlacedTeamCount == 0)
        {
            return;
        }

        _groups[(PlacedTeamCount - 1) % _groups.Count].RemoveLastPlacement();
    }
}
