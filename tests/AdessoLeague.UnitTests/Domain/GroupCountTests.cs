namespace AdessoLeague.UnitTests.Domain;

public sealed class GroupCountTests
{
    [Theory]
    [InlineData(4, 8)]
    [InlineData(8, 4)]
    public void Create_WithSupportedValue_ReturnsGroupCountWithMatchingGroupSize(int value, int expectedTeamsPerGroup)
    {
        var result = GroupCount.Create(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
        result.Value.TeamsPerGroup.Should().Be(expectedTeamsPerGroup);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(-4)]
    public void Create_WithUnsupportedValue_ReturnsValidationFailure(int value)
    {
        var result = GroupCount.Create(value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("GroupCount.Unsupported");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_WithUnsupportedValue_DoesNotExposeAValue()
    {
        var result = GroupCount.Create(5);

        var readValue = () => result.Value;

        readValue.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void TeamsPerGroup_ForAnySupportedValue_MultipliesBackToThePoolSize(int value)
    {
        var groupCount = GroupCount.Create(value).Value;

        (groupCount.Value * groupCount.TeamsPerGroup).Should().Be(GroupCount.TotalTeams);
    }
}
