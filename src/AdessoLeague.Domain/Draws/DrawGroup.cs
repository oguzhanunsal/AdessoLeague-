using AdessoLeague.Domain.Leagues;

namespace AdessoLeague.Domain.Draws;

public sealed class DrawGroup
{
    private readonly List<DrawGroupTeam> _teams = [];

    // Materialization only; the ORM assigns every member right after calling it.
    private DrawGroup() => Name = null!;

    private DrawGroup(Guid id, Guid drawId, GroupName name, int ordinal)
    {
        Id = id;
        DrawId = drawId;
        Name = name;
        Ordinal = ordinal;
    }

    public Guid Id { get; private set; }

    public Guid DrawId { get; private set; }

    public GroupName Name { get; private set; }

    /// <summary>Zero-based position of the group in the draw; "A" is 0.</summary>
    public int Ordinal { get; private set; }

    public IReadOnlyCollection<DrawGroupTeam> Teams => _teams.AsReadOnly();

    internal static DrawGroup Create(Guid id, Guid drawId, GroupName name, int ordinal) =>
        new(id, drawId, name, ordinal);

    internal bool Contains(Guid teamId) => _teams.Exists(team => team.TeamId == teamId);

    internal DrawGroupTeam Place(Team team)
    {
        var placement = DrawGroupTeam.Create(Guid.NewGuid(), Id, DrawId, team, _teams.Count + 1);
        _teams.Add(placement);

        return placement;
    }

    internal void RemoveLastPlacement() => _teams.RemoveAt(_teams.Count - 1);
}
