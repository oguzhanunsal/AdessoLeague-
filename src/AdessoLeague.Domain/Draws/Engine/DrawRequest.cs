using AdessoLeague.Domain.Leagues;

namespace AdessoLeague.Domain.Draws.Engine;

public sealed record DrawRequest(
    Guid DrawId,
    DrawnBy DrawnBy,
    GroupCount GroupCount,
    IReadOnlyCollection<Team> Pool,
    DateTime CreatedAtUtc);
