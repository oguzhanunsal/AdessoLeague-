using AdessoLeague.Domain.Draws.Engine;
using AdessoLeague.UnitTests.Domain;
using Xunit.Sdk;

namespace AdessoLeague.UnitTests.Draws;

public sealed class DrawEngineTests
{
    private const int FuzzSeedCount = 10_000;

    private const int FuzzSeedBatchSize = 100;

    private static readonly DateTime CreatedAtUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly IReadOnlyList<Guid> ExpectedTeamIds =
        LeagueTestData.Teams.Select(team => team.Id).Order().ToList();

    private static readonly IReadOnlyList<Guid> ExpectedCountryIds =
        LeagueTestData.Countries.Select(country => country.Id).Order().ToList();

    public static TheoryData<int, int> FuzzSeedBatches { get; } = BuildFuzzSeedBatches();

    [Theory]
    [MemberData(nameof(FuzzSeedBatches))]
    public void Execute_WithFourGroups_ProducesValidDraw(int firstSeed, int seedCount)
    {
        for (var seed = firstSeed; seed < firstSeed + seedCount; seed++)
        {
            var engine = NewEngine(seed);

            var result = engine.Run(NewRequest(4));

            result.IsSuccess.Should().BeTrue("seed {0} must produce a valid draw", seed);
            AssertValidDraw(result.Value, 4, seed);
        }
    }

    [Theory]
    [MemberData(nameof(FuzzSeedBatches))]
    public void Execute_WithEightGroups_ProducesValidDraw(int firstSeed, int seedCount)
    {
        for (var seed = firstSeed; seed < firstSeed + seedCount; seed++)
        {
            var engine = NewEngine(seed);

            var result = engine.Run(NewRequest(8));

            result.IsSuccess.Should().BeTrue("seed {0} must produce a valid draw", seed);
            AssertValidDraw(result.Value, 8, seed);
        }
    }

    [Fact]
    public void Execute_WithSameSeed_ProducesIdenticalDraw()
    {
        var engine = NewEngine(20260803);
        var other = NewEngine(20260803);

        var first = engine.Run(NewRequest(8));
        var second = other.Run(NewRequest(8));

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        Signature(second.Value).Should().Equal(Signature(first.Value));
        second.Value.Seed.Should().Be(first.Value.Seed);
    }

    [Fact]
    public void Execute_WithDifferentSeeds_ProducesDifferentDraws()
    {
        var signatures = new List<string>(100);

        for (var seed = 0; seed < 100; seed++)
        {
            var result = NewEngine(seed).Run(NewRequest(8));

            result.IsSuccess.Should().BeTrue();
            signatures.Add(string.Join('|', Signature(result.Value)));
        }

        signatures.Distinct().Should().HaveCountGreaterThan(95);
    }

    private static void AssertValidDraw(Draw draw, int groupCount, int seed)
    {
        try
        {
            AssertInvariants(draw, groupCount);
        }
        catch (XunitException failure)
        {
            throw new XunitException(
                FormattableString.Invariant($"Seed {seed} broke a draw invariant: {failure.Message}"),
                failure);
        }
    }

    private static void AssertInvariants(Draw draw, int groupCount)
    {
        var groupSize = GroupCount.TotalTeams / groupCount;
        var groups = draw.Groups.ToList();
        var placements = groups.SelectMany(group => group.Teams).ToList();

        placements.Should().HaveCount(GroupCount.TotalTeams);
        placements.Select(placement => placement.TeamId).Order().Should().Equal(ExpectedTeamIds);

        groups.Should().HaveCount(groupCount);
        groups.Select(group => group.Teams.Count).Should().Equal(Enumerable.Repeat(groupSize, groupCount));
        groups.Select(group => group.Name.Value).Should().Equal(GroupName.Sequence(groupCount).Select(name => name.Value));
        groups.Select(group => group.Ordinal).Should().Equal(Enumerable.Range(0, groupCount));

        foreach (var group in groups)
        {
            var countryIds = group.Teams.Select(placement => placement.Team.CountryId).ToList();

            countryIds.Distinct().Should().HaveCount(groupSize);
            group.Teams.Select(placement => placement.Position).Should().Equal(Enumerable.Range(1, groupSize));

            if (groupCount == 4)
            {
                countryIds.Order().Should().Equal(ExpectedCountryIds);
            }
        }

        groups
            .SelectMany(group => group.Teams.Select(placement => (placement.Position, group.Ordinal)))
            .OrderBy(slot => slot.Position)
            .ThenBy(slot => slot.Ordinal)
            .Should()
            .Equal(Enumerable.Range(0, GroupCount.TotalTeams)
                .Select(flatIndex => ((flatIndex / groupCount) + 1, flatIndex % groupCount)));
    }

    private static IReadOnlyList<string> Signature(Draw draw) => draw.Groups
        .SelectMany(group => group.Teams.Select(placement =>
            FormattableString.Invariant($"{group.Ordinal}:{placement.Position}:{placement.TeamId}")))
        .ToList();

    private static DrawEngine NewEngine(int seed) =>
        new(new BacktrackingRoundRobinStrategy(new SeededTestRandomProvider(seed), [new OneTeamPerCountryRule()]));

    private static DrawRequest NewRequest(int groupCount) => new(
        Guid.NewGuid(),
        DrawnBy.Create("Oğuzhan", "Ünsal").Value,
        GroupCount.Create(groupCount).Value,
        LeagueTestData.Teams,
        CreatedAtUtc);

    private static TheoryData<int, int> BuildFuzzSeedBatches()
    {
        var batches = new TheoryData<int, int>();

        for (var firstSeed = 0; firstSeed < FuzzSeedCount; firstSeed += FuzzSeedBatchSize)
        {
            batches.Add(firstSeed, Math.Min(FuzzSeedBatchSize, FuzzSeedCount - firstSeed));
        }

        return batches;
    }
}
