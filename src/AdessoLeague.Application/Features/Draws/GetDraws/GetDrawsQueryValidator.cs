using FluentValidation;

namespace AdessoLeague.Application.Features.Draws.GetDraws;

public sealed class GetDrawsQueryValidator : AbstractValidator<GetDrawsQuery>
{
    public GetDrawsQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(GetDrawsQuery.MaxPageSize);
    }
}
