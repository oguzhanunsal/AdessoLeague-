namespace AdessoLeague.UnitTests.Domain;

public sealed class DrawTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void Create_WithSupportedGroupCount_CreatesThatManyEmptyGroups(int groupCount)
    {
        var draw = NewDraw(groupCount);

        draw.Groups.Should().HaveCount(groupCount);
        draw.Groups.Should().OnlyContain(group => group.Teams.Count == 0);
        draw.PlacedTeamCount.Should().Be(0);
        draw.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void Create_WithEightGroups_NamesThemAThroughHInOrder()
    {
        var draw = NewDraw(8);

        draw.Groups.Select(group => group.Name.Value).Should().Equal(
            "A", "B", "C", "D", "E", "F", "G", "H");
        draw.Groups.Select(group => group.Ordinal).Should().Equal(0, 1, 2, 3, 4, 5, 6, 7);
    }

    [Fact]
    public void Create_Always_KeepsTheSuppliedSeedAndTimestamp()
    {
        var draw = NewDraw(8);

        draw.Seed.Should().Be(42);
        draw.CreatedAtUtc.Should().Be(CreatedAtUtc);
        draw.DrawnBy.FullName.Should().Be("Oğuzhan Ünsal");
    }

    [Fact]
    public void PlaceTeam_IntoTheNextGroupInOrder_Succeeds()
    {
        var draw = NewDraw(8);
        var teams = Pool();

        draw.PlaceTeam(0, teams[0]).IsSuccess.Should().BeTrue();
        draw.PlaceTeam(1, teams[1]).IsSuccess.Should().BeTrue();
        draw.PlacedTeamCount.Should().Be(2);
    }

    [Fact]
    public void PlaceTeam_SkippingAGroup_ReturnsValidationFailure()
    {
        var draw = NewDraw(8);
        var teams = Pool();

        var result = draw.PlaceTeam(1, teams[0]);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Draw.OutOfOrderPlacement");
        draw.PlacedTeamCount.Should().Be(0);
    }

    [Fact]
    public void PlaceTeam_FillingOneGroupBeforeTheOthers_ReturnsValidationFailure()
    {
        var draw = NewDraw(8);
        var teams = Pool();

        draw.PlaceTeam(0, teams[0]).IsSuccess.Should().BeTrue();

        var result = draw.PlaceTeam(0, teams[1]);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Draw.OutOfOrderPlacement");
    }

    [Fact]
    public void PlaceTeam_WithAnOrdinalOutsideTheDraw_ReturnsValidationFailure()
    {
        var draw = NewDraw(4);

        var result = draw.PlaceTeam(4, Pool()[0]);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Draw.UnknownGroup");
    }

    [Fact]
    public void PlaceTeam_WithATeamAlreadyPlaced_ReturnsValidationFailure()
    {
        var draw = NewDraw(4);
        var teams = Pool();

        draw.PlaceTeam(0, teams[0]).IsSuccess.Should().BeTrue();
        draw.PlaceTeam(1, teams[1]).IsSuccess.Should().BeTrue();
        draw.PlaceTeam(2, teams[2]).IsSuccess.Should().BeTrue();

        var result = draw.PlaceTeam(3, teams[0]);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Draw.DuplicateTeam");
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void PlaceTeam_ForTheWholePoolInRoundRobinOrder_CompletesTheDraw(int groupCount)
    {
        var draw = FilledDraw(groupCount);

        draw.IsComplete.Should().BeTrue();
        draw.PlacedTeamCount.Should().Be(32);
        draw.Groups.Should().OnlyContain(group => group.Teams.Count == 32 / groupCount);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void PlaceTeam_ForTheWholePool_NumbersPositionsFromOneWithinEachGroup(int groupCount)
    {
        var draw = FilledDraw(groupCount);

        var expectedPositions = Enumerable.Range(1, 32 / groupCount);
        foreach (var group in draw.Groups)
        {
            group.Teams.Select(team => team.Position).Should().Equal(expectedPositions);
        }
    }

    [Fact]
    public void PlaceTeam_ForTheWholePool_RecordsTheRoundRobinOrderInThePositions()
    {
        var pool = Pool();
        var draw = FilledDraw(8);

        var groups = draw.Groups.ToList();
        for (var flatIndex = 0; flatIndex < 32; flatIndex++)
        {
            var group = groups[flatIndex % 8];
            var placement = group.Teams.Single(team => team.Position == (flatIndex / 8) + 1);

            placement.TeamId.Should().Be(pool[flatIndex].Id);
        }
    }

    private static Draw FilledDraw(int groupCount)
    {
        var draw = NewDraw(groupCount);
        var pool = Pool();

        for (var flatIndex = 0; flatIndex < pool.Count; flatIndex++)
        {
            draw.PlaceTeam(flatIndex % groupCount, pool[flatIndex]).IsSuccess.Should().BeTrue();
        }

        return draw;
    }

    private static Draw NewDraw(int groupCount) => Draw.Create(
        Guid.NewGuid(),
        DrawnBy.Create("Oğuzhan", "Ünsal").Value,
        GroupCount.Create(groupCount).Value,
        seed: 42,
        CreatedAtUtc);

    private static IReadOnlyList<Team> Pool() => LeagueTestData.Teams;
}
