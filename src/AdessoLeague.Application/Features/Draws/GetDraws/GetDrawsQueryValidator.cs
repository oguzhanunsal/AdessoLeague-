using AdessoLeague.Application.Options;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace AdessoLeague.Application.Features.Draws.GetDraws;

public sealed class GetDrawsQueryValidator : AbstractValidator<GetDrawsQuery>
{
    public GetDrawsQueryValidator(IOptions<DrawOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, options.Value.MaxPageSize)
            .When(query => query.PageSize.HasValue);
    }
}
