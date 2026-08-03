namespace AdessoLeague.IntegrationTests.Draws;

/// <summary>
/// Guards the fixture's rate-limit override. Without it the shared container run would start
/// returning 429 after the production budget of 30 draws per minute, turning tests red at random.
/// </summary>
[Collection(LeagueApiCollection.Name)]
public sealed class DrawRateLimitTests(LeagueApiFactory factory) : IAsyncLifetime
{
    private const int ProductionRequestsPerMinute = 30;

    private readonly CancellationToken _cancellation = factory.Cancellation;

    public Task InitializeAsync() => factory.ResetAsync(factory.Cancellation);

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateDraw_MoreRequestsThanTheProductionRateLimit_AreNotThrottled()
    {
        var statuses = new List<HttpStatusCode>();

        for (var request = 0; request < ProductionRequestsPerMinute + 5; request++)
        {
            var response = await factory.Client.PostAsJsonAsync(
                DrawEndpoint.Path,
                DrawEndpoint.Payload(8),
                _cancellation);

            statuses.Add(response.StatusCode);
        }

        statuses.Should().HaveCount(ProductionRequestsPerMinute + 5);
        statuses.Should().OnlyContain(status => status == HttpStatusCode.Created);

        var draws = await factory.ReadDrawsAsync(_cancellation);
        draws.Should().HaveCount(ProductionRequestsPerMinute + 5);
    }
}
