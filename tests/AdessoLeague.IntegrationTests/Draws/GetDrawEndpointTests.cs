using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdessoLeague.IntegrationTests.Draws;

[Collection(LeagueApiCollection.Name)]
public sealed class GetDrawEndpointTests(LeagueApiFactory factory) : IAsyncLifetime
{
    private readonly CancellationToken _cancellation = factory.Cancellation;

    public Task InitializeAsync() => factory.ResetAsync(factory.Cancellation);

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetDrawById_AfterCreate_ReturnsSameGroups()
    {
        var created = await CreateAsync(8, "Oğuzhan", "Ünsal");

        var response = await factory.Client.GetAsync(DrawEndpoint.For(created.Id), _cancellation);
        var fetched = await response.Content.ReadFromJsonAsync<DrawResponse>(_cancellation);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        fetched!.Id.Should().Be(created.Id);
        fetched.GroupCount.Should().Be(created.GroupCount);
        fetched.DrawnBy.Should().Be(created.DrawnBy);
        fetched.Groups.Should().BeEquivalentTo(created.Groups, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetDrawById_WithUnknownId_Returns404()
    {
        var unknownId = Guid.NewGuid();

        var response = await factory.Client.GetAsync(DrawEndpoint.For(unknownId), _cancellation);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellation);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        problem!.Status.Should().Be(StatusCodes.Status404NotFound);
        problem.Title.Should().Be("Draw.NotFound");
        problem.Type.Should().NotBeNullOrWhiteSpace();
        problem.Instance.Should().Be(DrawEndpoint.For(unknownId));
        problem.Detail.Should().Contain(unknownId.ToString());
    }

    [Fact]
    public async Task GetDraws_ReturnsHistoryOrderedByCreatedAtDesc()
    {
        var first = await CreateAsync(8, "Ada", "Lovelace");
        var second = await CreateAsync(4, "Grace", "Hopper");
        var third = await CreateAsync(8, "Alan", "Turing");

        var response = await factory.Client.GetAsync(DrawEndpoint.Path, _cancellation);
        var page = await response.Content.ReadFromJsonAsync<PagedList<DrawSummaryResponse>>(_cancellation);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        page!.TotalCount.Should().Be(3);
        page.Items.Select(item => item.Id).Should().Equal(third.Id, second.Id, first.Id);
        page.Items.Select(item => item.DrawnBy.FirstName).Should().Equal("Alan", "Grace", "Ada");
        page.Items.Select(item => item.GroupCount).Should().Equal(8, 4, 8);
        page.Items.Select(item => item.CreatedAtUtc).Should().BeInDescendingOrder();

        var persisted = await factory.ReadDrawsAsync(_cancellation);
        persisted.Select(draw => draw.Id).Should().Equal(first.Id, second.Id, third.Id);
    }

    private async Task<DrawResponse> CreateAsync(int groupCount, string firstName, string lastName)
    {
        var response = await factory.Client.PostAsJsonAsync(
            DrawEndpoint.Path,
            DrawEndpoint.Payload(groupCount, firstName, lastName),
            _cancellation);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (await response.Content.ReadFromJsonAsync<DrawResponse>(_cancellation))!;
    }
}
