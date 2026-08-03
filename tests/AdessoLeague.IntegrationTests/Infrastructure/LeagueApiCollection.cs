namespace AdessoLeague.IntegrationTests.Infrastructure;

/// <summary>
/// One PostgreSQL container for the whole run. Every test class joins this collection, so the tests
/// run one after another and each one starts from a truncated database.
/// </summary>
[CollectionDefinition(Name)]
public sealed class LeagueApiCollection : ICollectionFixture<LeagueApiFactory>
{
    public const string Name = "league-api";
}
