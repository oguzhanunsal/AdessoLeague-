using AdessoLeague.Application.Options;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace AdessoLeague.Application.Features.Draws.GetDraws;

public sealed class GetDrawsQueryValidator : AbstractValidator<GetDrawsQuery>
{
    public GetDrawsQueryValidator(IOptions<DrawOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var defaultPageSize = options.Value.DefaultPageSize;

        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            // The offset is computed as (page - 1) * pageSize; an unbounded page overflows int and
            // reaches the database as a negative OFFSET.
            .Must((query, page) => (long)(page - 1) * (query.PageSize ?? defaultPageSize) <= int.MaxValue)
            .WithMessage("'Page' is too large for the requested page size.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, options.Value.MaxPageSize)
            .When(query => query.PageSize.HasValue);
    }
}
