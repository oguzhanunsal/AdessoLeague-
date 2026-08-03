using AdessoLeague.Application.Features.Draws.GetDraws;
using AdessoLeague.Application.Options;
using Microsoft.Extensions.Options;

namespace AdessoLeague.UnitTests.Application;

public sealed class GetDrawsQueryValidatorTests
{
    private static readonly DrawOptions Options = new();

    private readonly GetDrawsQueryValidator _validator =
        new(Microsoft.Extensions.Options.Options.Create(Options));

    [Theory]
    [InlineData(1, null)]
    [InlineData(1, 1)]
    [InlineData(7, 20)]
    [InlineData(1, 100)]
    public void Validate_WithAcceptableParameters_Succeeds(int page, int? pageSize)
    {
        _validator.Validate(new GetDrawsQuery(page, pageSize)).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Validate_WithPageBelowOne_Fails(int page)
    {
        var result = _validator.Validate(new GetDrawsQuery(page, 20));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(failure => failure.PropertyName == nameof(GetDrawsQuery.Page));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(int.MaxValue)]
    public void Validate_WithPageSizeOutsideTheAllowedRange_Fails(int pageSize)
    {
        var result = _validator.Validate(new GetDrawsQuery(1, pageSize));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(failure => failure.PropertyName == nameof(GetDrawsQuery.PageSize));
    }

    [Fact]
    public void Validate_WithAPageThatWouldOverflowTheOffset_Fails()
    {
        // (page - 1) * pageSize is computed as int; without the guard this reaches the database as
        // a negative OFFSET and surfaces as a 500 instead of a 400.
        var result = _validator.Validate(new GetDrawsQuery(int.MaxValue, 100));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(failure => failure.PropertyName == nameof(GetDrawsQuery.Page));
    }

    [Fact]
    public void Validate_WithTheLargestNonOverflowingPage_Succeeds()
    {
        var page = (int.MaxValue / Options.MaxPageSize) + 1;

        _validator.Validate(new GetDrawsQuery(page, Options.MaxPageSize)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithoutAPageSize_UsesTheConfiguredDefaultForTheOverflowCheck()
    {
        var page = (int.MaxValue / Options.DefaultPageSize) + 2;

        _validator.Validate(new GetDrawsQuery(page, null)).IsValid.Should().BeFalse();
    }
}
