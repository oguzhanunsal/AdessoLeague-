using System.Text.Json;

namespace AdessoLeague.IntegrationTests.Draws;

[Collection(LeagueApiCollection.Name)]
public sealed class DrawResponseSchemaTests(LeagueApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetAsync(factory.Cancellation);

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ResponseSchema_MatchesChallengeExample()
    {
        var cancellation = factory.Cancellation;

        var created = await factory.Client.PostAsJsonAsync(
            DrawEndpoint.Path,
            DrawEndpoint.Payload(8),
            cancellation);
        var createdJson = await created.Content.ReadAsStringAsync(cancellation);

        created.StatusCode.Should().Be(HttpStatusCode.Created);

        using var createdDocument = JsonDocument.Parse(createdJson);
        var root = createdDocument.RootElement;

        AssertChallengeShape(root, expectedGroups: 8, expectedTeamsPerGroup: 4);
        root.EnumerateObject().Select(property => property.Name).Should()
            .Contain(new[] { "id", "drawnBy", "groupCount", "createdAtUtc" });

        var id = root.GetProperty("id").GetGuid();
        var fetched = await factory.Client.GetAsync(DrawEndpoint.For(id), cancellation);
        var fetchedJson = await fetched.Content.ReadAsStringAsync(cancellation);

        using var fetchedDocument = JsonDocument.Parse(fetchedJson);

        AssertChallengeShape(fetchedDocument.RootElement, expectedGroups: 8, expectedTeamsPerGroup: 4);
    }

    private static void AssertChallengeShape(JsonElement root, int expectedGroups, int expectedTeamsPerGroup)
    {
        root.TryGetProperty("groups", out var groups).Should().BeTrue();
        groups.ValueKind.Should().Be(JsonValueKind.Array);
        groups.GetArrayLength().Should().Be(expectedGroups);

        foreach (var group in groups.EnumerateArray())
        {
            group.EnumerateObject().Select(property => property.Name).Should().Equal("groupName", "teams");
            group.GetProperty("groupName").ValueKind.Should().Be(JsonValueKind.String);
            group.GetProperty("groupName").GetString().Should().MatchRegex("^[A-H]$");

            var teams = group.GetProperty("teams");
            teams.ValueKind.Should().Be(JsonValueKind.Array);
            teams.GetArrayLength().Should().Be(expectedTeamsPerGroup);

            foreach (var team in teams.EnumerateArray())
            {
                team.EnumerateObject().Select(property => property.Name).Should().Equal("name");
                team.GetProperty("name").ValueKind.Should().Be(JsonValueKind.String);
                team.GetProperty("name").GetString().Should().NotBeNullOrWhiteSpace();
            }
        }
    }
}
