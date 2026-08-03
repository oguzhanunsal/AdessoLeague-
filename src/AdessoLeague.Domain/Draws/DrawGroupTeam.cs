using AdessoLeague.Domain.Leagues;

namespace AdessoLeague.Domain.Draws;

public sealed class DrawGroupTeam
{
    private DrawGroupTeam(Guid id, Guid drawGroupId, Team team, int position)
    {
        Id = id;
        DrawGroupId = drawGroupId;
        TeamId = team.Id;
        Team = team;
        Position = position;
    }

    public Guid Id { get; private set; }

    public Guid DrawGroupId { get; private set; }

    public Guid TeamId { get; private set; }

    public Team Team { get; private set; }

    /// <summary>1-based slot within the group; records the order the draw produced.</summary>
    public int Position { get; private set; }

    internal static DrawGroupTeam Create(Guid id, Guid drawGroupId, Team team, int position) =>
        new(id, drawGroupId, team, position);
}
