using AdessoLeague.Application.Abstractions.Persistence;
using AdessoLeague.Application.Contracts;
using AdessoLeague.Infrastructure.Persistence;

namespace AdessoLeague.Infrastructure.Repositories;

internal sealed class DrawQueries(LeagueDbContext context) : IDrawQueries
{
    public async Task<DrawResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var draw = await context.Draws
            .AsNoTracking()
            .Where(draw => draw.Id == id)
            .Select(draw => new
            {
                draw.Id,
                draw.DrawnBy.FirstName,
                draw.DrawnBy.LastName,
                draw.GroupCount,
                draw.CreatedAtUtc,
                Groups = draw.Groups
                    .OrderBy(group => group.Ordinal)
                    .Select(group => new
                    {
                        group.Name,
                        Teams = group.Teams
                            .OrderBy(placement => placement.Position)
                            .Select(placement => placement.Team.Name)
                            .ToList(),
                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Value-converted properties cannot be unwrapped in SQL, so the strings are taken after the
        // single round trip. Folding this into the Select above breaks EF's translation.
        return draw is null
            ? null
            : new DrawResponse(
                draw.Id,
                draw.Groups
                    .Select(group => new GroupResponse(
                        group.Name.Value,
                        group.Teams.Select(name => new TeamResponse(name)).ToList()))
                    .ToList(),
                new DrawnByResponse(draw.FirstName, draw.LastName),
                draw.GroupCount.Value,
                draw.CreatedAtUtc);
    }

    public async Task<PagedList<DrawSummaryResponse>> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var draws = context.Draws.AsNoTracking();

        var totalCount = await draws.CountAsync(cancellationToken);

        var rows = await draws
            .OrderByDescending(draw => draw.CreatedAtUtc)
            .ThenByDescending(draw => draw.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(draw => new
            {
                draw.Id,
                draw.DrawnBy.FirstName,
                draw.DrawnBy.LastName,
                draw.GroupCount,
                draw.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new DrawSummaryResponse(
                row.Id,
                new DrawnByResponse(row.FirstName, row.LastName),
                row.GroupCount.Value,
                row.CreatedAtUtc))
            .ToList();

        return new PagedList<DrawSummaryResponse>(items, page, pageSize, totalCount);
    }
}
