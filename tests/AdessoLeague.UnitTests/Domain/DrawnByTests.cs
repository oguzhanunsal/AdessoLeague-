namespace AdessoLeague.UnitTests.Domain;

public sealed class DrawnByTests
{
    [Fact]
    public void Create_WithValidNames_ReturnsDrawnBy()
    {
        var result = DrawnBy.Create("Oğuzhan", "Ünsal");

        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be("Oğuzhan");
        result.Value.LastName.Should().Be("Ünsal");
        result.Value.FullName.Should().Be("Oğuzhan Ünsal");
    }

    [Fact]
    public void Create_WithSurroundingWhitespace_TrimsIt()
    {
        var result = DrawnBy.Create("  Oğuzhan\t", "\r\nÜnsal  ");

        result.Value.FirstName.Should().Be("Oğuzhan");
        result.Value.LastName.Should().Be("Ünsal");
    }

    [Fact]
    public void Create_WithRepeatedInnerWhitespace_CollapsesItToSingleSpaces()
    {
        var result = DrawnBy.Create("Ahmet   Can", "Yıldırım  Öz");

        result.Value.FirstName.Should().Be("Ahmet Can");
        result.Value.LastName.Should().Be("Yıldırım Öz");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void Create_WithBlankFirstName_ReturnsValidationFailure(string? firstName)
    {
        var result = DrawnBy.Create(firstName, "Ünsal");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DrawnBy.FirstNameEmpty");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankLastName_ReturnsValidationFailure(string? lastName)
    {
        var result = DrawnBy.Create("Oğuzhan", lastName);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DrawnBy.LastNameEmpty");
    }

    [Fact]
    public void Create_WithFirstNameAtTheLengthLimit_Succeeds()
    {
        var result = DrawnBy.Create(new string('a', DrawnBy.MaxNameLength), "Ünsal");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithFirstNameOverTheLengthLimit_ReturnsValidationFailure()
    {
        var result = DrawnBy.Create(new string('a', DrawnBy.MaxNameLength + 1), "Ünsal");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DrawnBy.FirstNameTooLong");
    }

    [Fact]
    public void Create_WithLastNameOverTheLengthLimit_ReturnsValidationFailure()
    {
        var result = DrawnBy.Create("Oğuzhan", new string('a', DrawnBy.MaxNameLength + 1));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DrawnBy.LastNameTooLong");
    }

    [Fact]
    public void Create_WithNamesLongOnlyBeforeTrimming_Succeeds()
    {
        var padded = "  " + new string('a', DrawnBy.MaxNameLength) + "  ";

        DrawnBy.Create(padded, "Ünsal").IsSuccess.Should().BeTrue();
    }
}
