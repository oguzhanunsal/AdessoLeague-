namespace AdessoLeague.UnitTests.Domain;

public sealed class GroupNameTests
{
    [Fact]
    public void Sequence_WithFour_ReturnsAThroughD()
    {
        GroupName.Sequence(4).Select(name => name.Value).Should().Equal("A", "B", "C", "D");
    }

    [Fact]
    public void Sequence_WithEight_ReturnsAThroughH()
    {
        GroupName.Sequence(8).Select(name => name.Value).Should().Equal(
            "A", "B", "C", "D", "E", "F", "G", "H");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(9)]
    public void Sequence_WithCountOutsideTheAlphabet_Throws(int count)
    {
        var sequence = () => GroupName.Sequence(count);

        sequence.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Sequence_CalledTwice_ProducesEqualNames()
    {
        GroupName.Sequence(8).Should().Equal(GroupName.Sequence(8));
    }

    [Fact]
    public void MaxGroups_Always_MatchesTheLargestSupportedGroupCount()
    {
        GroupName.MaxGroups.Should().Be(8);
    }
}
