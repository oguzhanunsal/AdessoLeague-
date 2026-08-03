namespace AdessoLeague.Infrastructure.Persistence.Seed;

public sealed record CountrySeed(Guid Id, string Name, IReadOnlyList<TeamSeed> Teams);

public sealed record TeamSeed(Guid Id, Guid CountryId, string Name);

/// <summary>
/// Feeds <c>HasData</c> in the initial migration. At runtime the draw reads the pool from the
/// database, never from here.
/// </summary>
public static class LeagueSeedData
{
    public const int CountryCount = 8;
    public const int TeamsPerCountry = 4;
    public const int TotalTeams = CountryCount * TeamsPerCountry;

    private static readonly (string Country, string[] Teams)[] Source =
    [
        ("Türkiye", ["Adesso İstanbul", "Adesso Ankara", "Adesso İzmir", "Adesso Antalya"]),
        ("Almanya", ["Adesso Berlin", "Adesso Frankfurt", "Adesso Münih", "Adesso Dortmund"]),
        ("Fransa", ["Adesso Paris", "Adesso Marsilya", "Adesso Nice", "Adesso Lyon"]),
        ("Hollanda", ["Adesso Amsterdam", "Adesso Rotterdam", "Adesso Lahey", "Adesso Eindhoven"]),
        ("Portekiz", ["Adesso Lisbon", "Adesso Porto", "Adesso Braga", "Adesso Coimbra"]),
        ("İtalya", ["Adesso Roma", "Adesso Milano", "Adesso Venedik", "Adesso Napoli"]),
        ("İspanya", ["Adesso Sevilla", "Adesso Madrid", "Adesso Barselona", "Adesso Granada"]),
        ("Belçika", ["Adesso Brüksel", "Adesso Brugge", "Adesso Gent", "Adesso Anvers"]),
    ];

    public static IReadOnlyList<CountrySeed> Countries { get; } = Build();

    public static IReadOnlyList<TeamSeed> Teams { get; } =
        Countries.SelectMany(country => country.Teams).ToList();

    private static IReadOnlyList<CountrySeed> Build()
    {
        var countries = new List<CountrySeed>(Source.Length);

        for (var countryIndex = 0; countryIndex < Source.Length; countryIndex++)
        {
            var (countryName, teamNames) = Source[countryIndex];
            var countryId = CountryId(countryIndex + 1);

            var teams = teamNames
                .Select((teamName, teamIndex) => new TeamSeed(
                    TeamId(countryIndex + 1, teamIndex + 1),
                    countryId,
                    teamName))
                .ToList();

            countries.Add(new CountrySeed(countryId, countryName, teams));
        }

        return countries;
    }

    // Derived from the 1-based ordinals so the migration produces identical rows on every machine.
    private static Guid CountryId(int countryOrdinal) =>
        new(FormattableString.Invariant($"00000000-0000-0000-0000-{countryOrdinal:D12}"));

    private static Guid TeamId(int countryOrdinal, int teamOrdinal) =>
        new(FormattableString.Invariant($"00000000-0000-0000-{countryOrdinal:D4}-{teamOrdinal:D12}"));
}
