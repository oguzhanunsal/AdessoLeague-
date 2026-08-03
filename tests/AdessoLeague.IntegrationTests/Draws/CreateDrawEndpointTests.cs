using AdessoLeague.Application.Abstractions.Persistence;
using AdessoLeague.Domain.Draws.Engine;
using AdessoLeague.Domain.Leagues;
using AdessoLeague.Domain.ValueObjects;
using AdessoLeague.Infrastructure.Randomization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdessoLeague.IntegrationTests.Draws;

[Collection(LeagueApiCollection.Name)]
public sealed class CreateDrawEndpointTests(LeagueApiFactory factory) : IAsyncLifetime
{
    private readonly CancellationToken _cancellation = factory.Cancellation;

    public Task InitializeAsync() => factory.ResetAsync(factory.Cancellation);

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateDraw_WithEightGroups_Returns201AndPersistsDraw()
    {
        var payload = DrawEndpoint.Payload(8);

        var response = await factory.Client.PostAsJsonAsync(DrawEndpoint.Path, payload, _cancellation);
        var body = await response.Content.ReadFromJsonAsync<DrawResponse>(_cancellation);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        body.Should().NotBeNull();
        response.Headers.Location!.AbsolutePath.Should().Be(DrawEndpoint.For(body!.Id));
        body.Groups.Select(group => group.GroupName).Should().Equal("A", "B", "C", "D", "E", "F", "G", "H");
        body.Groups.Should().OnlyContain(group => group.Teams.Count == 4);
        body.Groups.SelectMany(group => group.Teams).Should().HaveCount(32).And.OnlyHaveUniqueItems();

        var draws = await factory.ReadDrawsAsync(_cancellation);
        var persisted = draws.Should().ContainSingle().Subject;

        persisted.Id.Should().Be(body.Id);
        persisted.GroupCount.Should().Be(8);
        persisted.Groups.Select(group => group.Name).Should().Equal("A", "B", "C", "D", "E", "F", "G", "H");
        persisted.Groups.Select(group => group.Ordinal).Should().Equal(0, 1, 2, 3, 4, 5, 6, 7);
        persisted.Groups.Should().OnlyContain(group => group.Teams.Count == 4);
        persisted.Groups.Should().AllSatisfy(group =>
            group.Teams.Select(team => team.CountryId).Should().OnlyHaveUniqueItems());
        persisted.Groups.Should().AllSatisfy(group =>
            group.Teams.Select(team => team.Position).Should().Equal(1, 2, 3, 4));
        persisted.Groups.SelectMany(group => group.Teams).Select(team => team.Name)
            .Should().HaveCount(32).And.OnlyHaveUniqueItems();

        var placements = await factory.CountPlacementsAsync(_cancellation);
        placements.Should().Be(32);
    }

    [Fact]
    public async Task CreateDraw_WithFourGroups_EachGroupContainsAllEightCountries()
    {
        var payload = DrawEndpoint.Payload(4);

        var response = await factory.Client.PostAsJsonAsync(DrawEndpoint.Path, payload, _cancellation);
        var body = await response.Content.ReadFromJsonAsync<DrawResponse>(_cancellation);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        body!.Groups.Select(group => group.GroupName).Should().Equal("A", "B", "C", "D");
        body.Groups.Should().OnlyContain(group => group.Teams.Count == 8);

        var draws = await factory.ReadDrawsAsync(_cancellation);
        var persisted = draws.Should().ContainSingle().Subject;

        persisted.GroupCount.Should().Be(4);
        persisted.Groups.Should().HaveCount(4);
        persisted.Groups.Should().AllSatisfy(group =>
            group.Teams.Select(team => team.CountryId).Should().HaveCount(8).And.OnlyHaveUniqueItems());
        persisted.Groups.Should().AllSatisfy(group =>
            group.Teams.Select(team => team.Position).Should().Equal(1, 2, 3, 4, 5, 6, 7, 8));

        var placements = await factory.CountPlacementsAsync(_cancellation);
        placements.Should().Be(32);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateDraw_WithInvalidGroupCount_Returns400ProblemDetails(int groupCount)
    {
        var payload = DrawEndpoint.Payload(groupCount);

        var response = await factory.Client.PostAsJsonAsync(DrawEndpoint.Path, payload, _cancellation);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(_cancellation);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        // A [Produces("application/json")] on the controller silently rewrote this media type once;
        // asserting it keeps the RFC 9457 contract from regressing.
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        problem!.Status.Should().Be(StatusCodes.Status400BadRequest);
        problem.Title.Should().Be("One or more validation errors occurred.");
        problem.Instance.Should().Be(DrawEndpoint.Path);
        problem.Type.Should().NotBeNullOrWhiteSpace();
        problem.Errors.Should().ContainKey(nameof(CreateDrawPayload.GroupCount));
        problem.Errors[nameof(CreateDrawPayload.GroupCount)].Should().ContainMatch("*4, 8*");

        var draws = await factory.ReadDrawsAsync(_cancellation);
        draws.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateDraw_WithBlankDrawerName_Returns400()
    {
        var payload = DrawEndpoint.Payload(8, firstName: string.Empty, lastName: "   ");

        var response = await factory.Client.PostAsJsonAsync(DrawEndpoint.Path, payload, _cancellation);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(_cancellation);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        problem!.Errors.Should().ContainKey(nameof(DrawnByPayload.FirstName));
        problem.Errors.Should().ContainKey(nameof(DrawnByPayload.LastName));

        var draws = await factory.ReadDrawsAsync(_cancellation);
        draws.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateDraw_PersistsDrawerNameAndSeed()
    {
        var payload = DrawEndpoint.Payload(8, "Oğuzhan", "Ünsal");

        var response = await factory.Client.PostAsJsonAsync(DrawEndpoint.Path, payload, _cancellation);
        var body = await response.Content.ReadFromJsonAsync<DrawResponse>(_cancellation);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        body!.DrawnBy.Should().Be(new DrawnByResponse("Oğuzhan", "Ünsal"));

        var id = body.Id;
        var firstName = await factory.ScalarAsync<string>(
            $"SELECT drawn_by_first_name AS \"Value\" FROM draws WHERE id = {id}",
            _cancellation);
        var lastName = await factory.ScalarAsync<string>(
            $"SELECT drawn_by_last_name AS \"Value\" FROM draws WHERE id = {id}",
            _cancellation);
        var seed = await factory.ScalarAsync<int>(
            $"SELECT seed AS \"Value\" FROM draws WHERE id = {id}",
            _cancellation);

        firstName.Should().Be("Oğuzhan");
        lastName.Should().Be("Ünsal");

        var replayed = await ReplayAsync(seed, body.GroupCount);
        replayed.Should().BeEquivalentTo(Layout(body), options => options.WithStrictOrdering());
    }

    private static IReadOnlyList<IReadOnlyList<string>> Layout(DrawResponse draw) => draw.Groups
        .Select(group => (IReadOnlyList<string>)group.Teams.Select(team => team.Name).ToList())
        .ToList();

    // The stored seed is only worth a column if it reproduces the draw: replaying it over the same
    // pool has to rebuild the very same groups.
    private async Task<IReadOnlyList<IReadOnlyList<string>>> ReplayAsync(int seed, int groupCount)
    {
        var pool = await factory.ExecuteScopedAsync<ITeamRepository, IReadOnlyList<Team>>(
            (teams, token) => teams.GetPoolAsync(token),
            _cancellation);

        var engine = new DrawEngine(new BacktrackingRoundRobinStrategy(
            new SeededRandomProvider(seed),
            [new OneTeamPerCountryRule()]));

        var replay = engine.Run(new DrawRequest(
            Guid.NewGuid(),
            DrawnBy.Create("Oğuzhan", "Ünsal").Value,
            GroupCount.Create(groupCount).Value,
            pool,
            DateTime.UtcNow));

        replay.IsSuccess.Should().BeTrue();

        return replay.Value.Groups
            .OrderBy(group => group.Ordinal)
            .Select(group => (IReadOnlyList<string>)group.Teams
                .OrderBy(placement => placement.Position)
                .Select(placement => placement.Team.Name)
                .ToList())
            .ToList();
    }
}
