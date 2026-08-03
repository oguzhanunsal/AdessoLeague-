namespace AdessoLeague.Domain.Draws;

public sealed class DrawGroupTeam
{
    private DrawGroupTeam(Guid id, Guid drawGroupId, Guid teamId, int position)
    {
        Id = id;
        DrawGroupId = drawGroupId;
        TeamId = teamId;
        Position = position;
    }

    public Guid Id { get; private set; }

    public Guid DrawGroupId { get; private set; }

    public Guid TeamId { get; private set; }

    /// <summary>1-based slot within the group; records the order the draw produced.</summary>
    public int Position { get; private set; }

    internal static DrawGroupTeam Create(Guid id, Guid drawGroupId, Guid teamId, int position) =>
        new(id, drawGroupId, teamId, position);
}
