using AdessoLeague.Application.Features.Draws.GetDrawById;
using AdessoLeague.Application.Features.Draws.GetDraws;
using AdessoLeague.Application.Options;
using AdessoLeague.UnitTests.Domain;
using Microsoft.Extensions.Options;

namespace AdessoLeague.UnitTests.Application;

public sealed class GetDrawQueryHandlerTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 8, 3, 9, 30, 0, DateTimeKind.Utc);

    private static readonly IOptions<DrawOptions> Options = Microsoft.Extensions.Options.Options.Create(new DrawOptions());

    private readonly FakeDrawQueries _draws = new();

    [Fact]
    public async Task Handle_WithAnUnknownId_ReturnsNotFound()
    {
        var result = await new GetDrawByIdQueryHandler(_draws)
            .Handle(new GetDrawByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Draw.NotFound");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WithAStoredId_ReturnsTheDraw()
    {
        var draw = StoreDraw(8);

        var result = await new GetDrawByIdQueryHandler(_draws)
            .Handle(new GetDrawByIdQuery(draw.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(draw.Id);
        result.Value.Groups.Should().HaveCount(8);
        result.Value.Groups.SelectMany(group => group.Teams).Should().HaveCount(32);
    }

    [Fact]
    public async Task Handle_WithPagingParameters_PassesThemThroughUnchanged()
    {
        await new GetDrawsQueryHandler(_draws, Options)
            .Handle(new GetDrawsQuery(Page: 3, PageSize: 15), CancellationToken.None);

        _draws.LastPage.Should().Be(3);
        _draws.LastPageSize.Should().Be(15);
    }

    [Fact]
    public async Task Handle_WithAPageBeyondTheData_ReturnsAnEmptyPageWithTheRealTotal()
    {
        StoreDraw(8);
        StoreDraw(4);

        var result = await new GetDrawsQueryHandler(_draws, Options)
            .Handle(new GetDrawsQuery(Page: 5, PageSize: 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(2);
        result.Value.Page.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WithSeveralStoredDraws_ReportsTotalCountAndPageSize()
    {
        StoreDraw(8);
        StoreDraw(4);
        StoreDraw(8);

        var result = await new GetDrawsQueryHandler(_draws, Options)
            .Handle(new GetDrawsQuery(Page: 1, PageSize: 2), CancellationToken.None);

        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(3);
        result.Value.TotalPages.Should().Be(2);
        result.Value.HasNextPage.Should().BeTrue();
    }

    private Draw StoreDraw(int groupCount)
    {
        var draw = Draw.Create(
            Guid.NewGuid(),
            DrawnBy.Create("Oğuzhan", "Ünsal").Value,
            GroupCount.Create(groupCount).Value,
            seed: 1,
            CreatedAtUtc);

        var pool = LeagueTestData.Teams;
        for (var flatIndex = 0; flatIndex < pool.Count; flatIndex++)
        {
            draw.PlaceTeam(flatIndex % groupCount, pool[flatIndex]).IsSuccess.Should().BeTrue();
        }

        _draws.Store(draw);

        return draw;
    }
}
