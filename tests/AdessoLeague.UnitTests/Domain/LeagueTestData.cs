namespace AdessoLeague.UnitTests.Domain;

// Same shape as the seeded pool - 8 countries, 4 teams each - with neutral names, so domain tests
// do not depend on which clubs happen to be seeded.
internal static class LeagueTestData
{
    public static IReadOnlyList<Country> Countries { get; } = Build();

    public static IReadOnlyList<Team> Teams { get; } =
        Countries.SelectMany(country => country.Teams).ToList();

    private static IReadOnlyList<Country> Build() => Enumerable.Range(1, 8)
        .Select(countryOrdinal => Country.Create(
            CountryId(countryOrdinal),
            FormattableString.Invariant($"Country {countryOrdinal}"),
            Enumerable.Range(1, 4).Select(teamOrdinal => (
                Id: TeamId(countryOrdinal, teamOrdinal),
                Name: FormattableString.Invariant($"Team {countryOrdinal}-{teamOrdinal}")))))
        .ToList();

    private static Guid CountryId(int countryOrdinal) =>
        new(FormattableString.Invariant($"00000000-0000-0000-0000-{countryOrdinal:D12}"));

    private static Guid TeamId(int countryOrdinal, int teamOrdinal) =>
        new(FormattableString.Invariant($"00000000-0000-0000-{countryOrdinal:D4}-{teamOrdinal:D12}"));
}
