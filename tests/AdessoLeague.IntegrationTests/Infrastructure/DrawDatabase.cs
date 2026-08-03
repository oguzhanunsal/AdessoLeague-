using AdessoLeague.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AdessoLeague.IntegrationTests.Infrastructure;

internal sealed record PersistedTeam(string Name, Guid CountryId, int Position);

internal sealed record PersistedGroup(string Name, int Ordinal, IReadOnlyList<PersistedTeam> Teams);

internal sealed record PersistedDraw(
    Guid Id,
    string FirstName,
    string LastName,
    int GroupCount,
    int Seed,
    DateTime CreatedAtUtc,
    IReadOnlyList<PersistedGroup> Groups);

/// <summary>Reads what the API actually wrote, so assertions do not lean on the HTTP response alone.</summary>
internal static class DrawDatabase
{
    internal static Task<IReadOnlyList<PersistedDraw>> ReadDrawsAsync(
        this LeagueApiFactory factory,
        CancellationToken cancellationToken) =>
        factory.ExecuteDbAsync(ReadAsync, cancellationToken);

    internal static Task<int> CountPlacementsAsync(
        this LeagueApiFactory factory,
        CancellationToken cancellationToken) =>
        factory.ExecuteDbAsync(
            (context, token) => context.DrawGroupTeams.CountAsync(token),
            cancellationToken);

    internal static Task<TValue> ScalarAsync<TValue>(
        this LeagueApiFactory factory,
        FormattableString sql,
        CancellationToken cancellationToken) =>
        factory.ExecuteDbAsync(
            (context, token) => context.Database.SqlQuery<TValue>(sql).SingleAsync(token),
            cancellationToken);

    private static async Task<IReadOnlyList<PersistedDraw>> ReadAsync(
        LeagueDbContext context,
        CancellationToken cancellationToken)
    {
        var rows = await context.Draws
            .AsNoTracking()
            .OrderBy(draw => draw.CreatedAtUtc)
            .Select(draw => new
            {
                draw.Id,
                draw.DrawnBy.FirstName,
                draw.DrawnBy.LastName,
                draw.GroupCount,
                draw.Seed,
                draw.CreatedAtUtc,
                Groups = draw.Groups
                    .OrderBy(group => group.Ordinal)
                    .Select(group => new
                    {
                        group.Name,
                        group.Ordinal,
                        Teams = group.Teams
                            .OrderBy(placement => placement.Position)
                            .Select(placement => new
                            {
                                placement.Team.Name,
                                placement.Team.CountryId,
                                placement.Position,
                            })
                            .ToList(),
                    })
                    .ToList(),
            })
            .ToListAsync(cancellationToken);

        // The value converters on group_count and draw_groups.name cannot be unwrapped inside the
        // SQL projection, so they are read as value objects and flattened here.
        return rows
            .Select(row => new PersistedDraw(
                row.Id,
                row.FirstName,
                row.LastName,
                row.GroupCount.Value,
                row.Seed,
                row.CreatedAtUtc,
                row.Groups
                    .Select(group => new PersistedGroup(
                        group.Name.Value,
                        group.Ordinal,
                        group.Teams
                            .Select(team => new PersistedTeam(team.Name, team.CountryId, team.Position))
                            .ToList()))
                    .ToList()))
            .ToList();
    }
}
