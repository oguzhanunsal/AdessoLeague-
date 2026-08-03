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

    [Fact]
    public async Task CreateDraw_BeyondTheConfiguredLimit_Returns429WithAProblemDocument()
    {
        const int limit = 3;

        using var throttled = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("Draw:RequestsPerMinute", limit.ToString(CultureInfo.InvariantCulture)));
        using var client = throttled.CreateClient();

        HttpResponseMessage? rejected = null;
        for (var request = 0; request <= limit; request++)
        {
            rejected = await client.PostAsJsonAsync(DrawEndpoint.Path, DrawEndpoint.Payload(8), _cancellation);
        }

        rejected.Should().NotBeNull();
        rejected!.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        rejected.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problem = await rejected.Content.ReadFromJsonAsync<ProblemDetails>(_cancellation);

        problem.Should().NotBeNull();
        problem!.Status.Should().Be(StatusCodes.Status429TooManyRequests);
        problem.Title.Should().NotBeNullOrWhiteSpace();
    }
}
