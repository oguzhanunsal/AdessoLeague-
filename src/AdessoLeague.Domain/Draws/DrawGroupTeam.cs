using AdessoLeague.Domain.Leagues;

namespace AdessoLeague.Domain.Draws;

public sealed class DrawGroupTeam
{
    // Materialization only; the ORM assigns every member right after calling it.
    private DrawGroupTeam() => Team = null!;

    private DrawGroupTeam(Guid id, Guid drawGroupId, Guid drawId, Team team, int position)
    {
        Id = id;
        DrawGroupId = drawGroupId;
        DrawId = drawId;
        TeamId = team.Id;
        Team = team;
        Position = position;
    }

    public Guid Id { get; private set; }

    public Guid DrawGroupId { get; private set; }

    /// <summary>
    /// Denormalised from the owning group so "a team appears once per draw" can be a database
    /// unique index instead of an application-level check.
    /// </summary>
    public Guid DrawId { get; private set; }

    public Guid TeamId { get; private set; }

    public Team Team { get; private set; }

    /// <summary>1-based slot within the group; records the order the draw produced.</summary>
    public int Position { get; private set; }

    internal static DrawGroupTeam Create(Guid id, Guid drawGroupId, Guid drawId, Team team, int position) =>
        new(id, drawGroupId, drawId, team, position);
}
