using AdessoLeague.Application.Contracts;
using AdessoLeague.Domain.Draws;

namespace AdessoLeague.Application.Mapping;

public static class DrawMappingExtensions
{
    public static DrawResponse ToResponse(this Draw draw)
    {
        ArgumentNullException.ThrowIfNull(draw);

        return new DrawResponse(
            draw.Id,
            draw.Groups
                .OrderBy(group => group.Ordinal)
                .Select(group => new GroupResponse(
                    group.Name.Value,
                    group.Teams
                        .OrderBy(placement => placement.Position)
                        .Select(placement => new TeamResponse(placement.Team.Name))
                        .ToList()))
                .ToList(),
            draw.DrawnBy.ToResponse(),
            draw.GroupCount.Value,
            draw.CreatedAtUtc);
    }

    public static DrawSummaryResponse ToSummaryResponse(this Draw draw)
    {
        ArgumentNullException.ThrowIfNull(draw);

        return new DrawSummaryResponse(
            draw.Id,
            draw.DrawnBy.ToResponse(),
            draw.GroupCount.Value,
            draw.CreatedAtUtc);
    }

    private static DrawnByResponse ToResponse(this DrawnBy drawnBy) =>
        new(drawnBy.FirstName, drawnBy.LastName);
}
