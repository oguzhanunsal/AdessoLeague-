namespace AdessoLeague.IntegrationTests.Draws;

[Collection(LeagueApiCollection.Name)]
public sealed class ConcurrentDrawTests(LeagueApiFactory factory) : IAsyncLifetime
{
    private const int RequestCount = 10;

    private readonly CancellationToken _cancellation = factory.Cancellation;

    public Task InitializeAsync() => factory.ResetAsync(factory.Cancellation);

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateDraw_TenConcurrentRequests_AllSucceedAndPersistIndependently()
    {
        var payloads = Enumerable.Range(1, RequestCount)
            .Select(index => DrawEndpoint.Payload(
                index % 2 == 0 ? 4 : 8,
                FormattableString.Invariant($"Drawer{index}"),
                "Concurrent"))
            .ToList();

        var responses = await Task.WhenAll(payloads.Select(payload =>
            factory.Client.PostAsJsonAsync(DrawEndpoint.Path, payload, _cancellation)));
        var bodies = await Task.WhenAll(responses.Select(response =>
            response.Content.ReadFromJsonAsync<DrawResponse>(_cancellation)));

        responses.Should().OnlyContain(response => response.StatusCode == HttpStatusCode.Created);
        bodies.Should().HaveCount(RequestCount).And.NotContainNulls();
        bodies.Select(body => body!.Id).Should().OnlyHaveUniqueItems();
        bodies.Should().AllSatisfy(body =>
            body!.Groups.SelectMany(group => group.Teams).Should().HaveCount(32).And.OnlyHaveUniqueItems());

        var persisted = await factory.ReadDrawsAsync(_cancellation);

        persisted.Should().HaveCount(RequestCount);
        persisted.Select(draw => draw.Id).Should().BeEquivalentTo(bodies.Select(body => body!.Id));
        persisted.Select(draw => draw.FirstName).Should()
            .BeEquivalentTo(payloads.Select(payload => payload.DrawnBy.FirstName));
        persisted.Should().AllSatisfy(draw =>
            draw.Groups.SelectMany(group => group.Teams).Select(team => team.Name)
                .Should().HaveCount(32).And.OnlyHaveUniqueItems());
        persisted.Should().AllSatisfy(draw => draw.Groups.Should().AllSatisfy(group =>
            group.Teams.Select(team => team.CountryId).Should().OnlyHaveUniqueItems()));

        var placements = await factory.CountPlacementsAsync(_cancellation);
        placements.Should().Be(RequestCount * 32);
    }
}
