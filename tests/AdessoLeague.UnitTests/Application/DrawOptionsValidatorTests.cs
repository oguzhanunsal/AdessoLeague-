using AdessoLeague.Application.Options;

namespace AdessoLeague.UnitTests.Application;

public sealed class DrawOptionsValidatorTests
{
    private readonly DrawOptionsValidator _validator = new();

    [Fact]
    public void Validate_WithTheDefaults_Succeeds()
    {
        _validator.Validate(null, new DrawOptions()).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithANonPositiveMaxPageSize_Fails(int maxPageSize)
    {
        var result = _validator.Validate(null, new DrawOptions { MaxPageSize = maxPageSize, DefaultPageSize = 1 });

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(failure => failure.Contains(nameof(DrawOptions.MaxPageSize)));
    }

    [Fact]
    public void Validate_WithANonPositiveDefaultPageSize_Fails()
    {
        var result = _validator.Validate(null, new DrawOptions { DefaultPageSize = 0 });

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(failure => failure.Contains(nameof(DrawOptions.DefaultPageSize)));
    }

    [Fact]
    public void Validate_WithADefaultLargerThanTheMaximum_Fails()
    {
        var result = _validator.Validate(null, new DrawOptions { DefaultPageSize = 50, MaxPageSize = 20 });

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(failure => failure.Contains("cannot exceed"));
    }

    [Fact]
    public void Validate_WithANonPositiveRequestLimit_Fails()
    {
        var result = _validator.Validate(null, new DrawOptions { RequestsPerMinute = 0 });

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(failure => failure.Contains(nameof(DrawOptions.RequestsPerMinute)));
    }

    [Fact]
    public void Validate_WithSeveralBrokenSettings_ReportsAllOfThem()
    {
        var result = _validator.Validate(null, new DrawOptions
        {
            DefaultPageSize = 0,
            MaxPageSize = 0,
            RequestsPerMinute = 0,
        });

        result.Failures.Should().HaveCountGreaterThanOrEqualTo(3);
    }
}
