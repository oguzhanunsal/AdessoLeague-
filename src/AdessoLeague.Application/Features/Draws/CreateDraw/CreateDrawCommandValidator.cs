using FluentValidation;

namespace AdessoLeague.Application.Features.Draws.CreateDraw;

public sealed class CreateDrawCommandValidator : AbstractValidator<CreateDrawCommand>
{
    public CreateDrawCommandValidator()
    {
        RuleFor(command => command.GroupCount)
            .Must(GroupCount.SupportedValues.Contains)
            .WithMessage($"Group count must be one of {string.Join(", ", GroupCount.SupportedValues)}.");

        RuleFor(command => command.FirstName)
            .NotEmpty()
            .MaximumLength(DrawnBy.MaxNameLength);

        RuleFor(command => command.LastName)
            .NotEmpty()
            .MaximumLength(DrawnBy.MaxNameLength);
    }
}
