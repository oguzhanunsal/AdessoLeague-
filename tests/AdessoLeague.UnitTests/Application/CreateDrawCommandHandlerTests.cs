using AdessoLeague.Application.Features.Draws.CreateDraw;
using AdessoLeague.Domain.Draws.Engine;
using AdessoLeague.UnitTests.Domain;
using AdessoLeague.UnitTests.Draws;

namespace AdessoLeague.UnitTests.Application;

public sealed class CreateDrawCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 3, 9, 30, 0, TimeSpan.Zero);

    private readonly FakeDrawRepository _draws = new();
    private readonly FakeTeamRepository _teams = new(LeagueTestData.Teams);
    private readonly FakeUnitOfWork _unitOfWork = new();

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public async Task Handle_WithSupportedGroupCount_ReturnsResponseWithAllThirtyTwoTeams(int groupCount)
    {
        var result = await Handler().Handle(Command(groupCount), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Groups.Should().HaveCount(groupCount);
        result.Value.Groups.SelectMany(group => group.Teams).Should().HaveCount(32);
        result.Value.GroupCount.Should().Be(groupCount);
    }

    [Fact]
    public async Task Handle_WithEightGroups_NamesGroupsAThroughHInOrder()
    {
        var result = await Handler().Handle(Command(8), CancellationToken.None);

        result.Value.Groups.Select(group => group.GroupName).Should().Equal(
            "A", "B", "C", "D", "E", "F", "G", "H");
    }

    [Fact]
    public async Task Handle_WithValidCommand_PersistsTheDrawExactlyOnce()
    {
        var result = await Handler().Handle(Command(8), CancellationToken.None);

        _draws.Added.Should().ContainSingle();
        _draws.Added[0].Id.Should().Be(result.Value.Id);
        _unitOfWork.SaveCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithValidCommand_KeepsTheDrawnByAndTheInjectedClockReading()
    {
        var result = await Handler().Handle(Command(8), CancellationToken.None);

        result.Value.DrawnBy.FirstName.Should().Be("Oğuzhan");
        result.Value.DrawnBy.LastName.Should().Be("Ünsal");
        result.Value.CreatedAtUtc.Should().Be(Now.UtcDateTime);
    }

    [Fact]
    public async Task Handle_WithValidCommand_TrimsTheDrawnByName()
    {
        var result = await Handler().Handle(new CreateDrawCommand(8, "  Oğuzhan  ", " Ünsal "), CancellationToken.None);

        result.Value.DrawnBy.FirstName.Should().Be("Oğuzhan");
        result.Value.DrawnBy.LastName.Should().Be("Ünsal");
    }

    [Theory]
    [InlineData(5)]
    [InlineData(0)]
    [InlineData(-4)]
    public async Task Handle_WithUnsupportedGroupCount_FailsWithoutPersisting(int groupCount)
    {
        var result = await Handler().Handle(Command(groupCount), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("GroupCount.Unsupported");
        _draws.Added.Should().BeEmpty();
        _unitOfWork.SaveCount.Should().Be(0);
        _teams.Calls.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithBlankName_FailsWithoutPersisting()
    {
        var result = await Handler().Handle(new CreateDrawCommand(8, "   ", "Ünsal"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DrawnBy.FirstNameEmpty");
        _draws.Added.Should().BeEmpty();
        _unitOfWork.SaveCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithAnIncompletePool_FailsWithoutPersisting()
    {
        var handler = Handler(new FakeTeamRepository(LeagueTestData.Teams.Take(31).ToList()));

        var result = await handler.Handle(Command(8), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Draw.PoolSizeMismatch");
        _draws.Added.Should().BeEmpty();
        _unitOfWork.SaveCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithFourGroups_PutsEveryCountryInEveryGroupExactlyOnce()
    {
        var result = await Handler().Handle(Command(4), CancellationToken.None);

        var namesByCountry = LeagueTestData.Countries
            .ToDictionary(country => country.Id, country => country.Teams.Select(team => team.Name).ToHashSet());

        foreach (var group in result.Value.Groups)
        {
            var names = group.Teams.Select(team => team.Name).ToList();

            names.Should().HaveCount(8);
            namesByCountry.Values.Should().OnlyContain(countryTeams => countryTeams.Overlaps(names));
        }
    }

    private static CreateDrawCommand Command(int groupCount) => new(groupCount, "Oğuzhan", "Ünsal");

    private CreateDrawCommandHandler Handler(FakeTeamRepository? teams = null) => new(
        new DrawEngine(new BacktrackingRoundRobinStrategy(
            new SeededTestRandomProvider(20260803),
            [new OneTeamPerCountryRule()])),
        teams ?? _teams,
        _draws,
        _unitOfWork,
        new FixedTimeProvider(Now));
}
