using AdessoLeague.Application.Behaviors;
using AdessoLeague.Application.Contracts;
using AdessoLeague.Application.Features.Draws.CreateDraw;
using FluentValidation;

namespace AdessoLeague.UnitTests.Application;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithValidRequest_CallsTheHandler()
    {
        var handlerCalls = 0;

        var result = await Behavior().Handle(
            new CreateDrawCommand(8, "Oğuzhan", "Ünsal"),
            _ =>
            {
                handlerCalls++;

                return Task.FromResult(Result.Success(Response));
            },
            CancellationToken.None);

        handlerCalls.Should().Be(1);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNoValidators_CallsTheHandler()
    {
        var handlerCalls = 0;

        var behavior = new ValidationBehavior<CreateDrawCommand, Result<DrawResponse>>([]);

        await behavior.Handle(
            new CreateDrawCommand(5, string.Empty, string.Empty),
            _ =>
            {
                handlerCalls++;

                return Task.FromResult(Result.Success(Response));
            },
            CancellationToken.None);

        handlerCalls.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_NeverCallsTheHandler()
    {
        var handlerCalls = 0;

        var result = await Behavior().Handle(
            new CreateDrawCommand(5, "Oğuzhan", "Ünsal"),
            _ =>
            {
                handlerCalls++;

                return Task.FromResult(Result.Success(Response));
            },
            CancellationToken.None);

        handlerCalls.Should().Be(0);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ReturnsAValidationErrorWithoutThrowing()
    {
        var result = await Behavior().Handle(
            new CreateDrawCommand(5, "Oğuzhan", "Ünsal"),
            _ => Task.FromResult(Result.Success(Response)),
            CancellationToken.None);

        result.Error.Should().BeOfType<ValidationError>();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("Validation.Failed");
    }

    [Fact]
    public async Task Handle_WithSeveralBrokenRules_ReportsEveryFailure()
    {
        var result = await Behavior().Handle(
            new CreateDrawCommand(5, string.Empty, string.Empty),
            _ => Task.FromResult(Result.Success(Response)),
            CancellationToken.None);

        var failures = result.Error.Should().BeOfType<ValidationError>().Subject.Errors;

        failures.Select(error => error.Code).Should().Contain(["GroupCount", "FirstName", "LastName"]);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_DoesNotExposeAValue()
    {
        var result = await Behavior().Handle(
            new CreateDrawCommand(5, "Oğuzhan", "Ünsal"),
            _ => Task.FromResult(Result.Success(Response)),
            CancellationToken.None);

        var readValue = () => result.Value;

        readValue.Should().Throw<InvalidOperationException>();
    }

    private static DrawResponse Response { get; } = new(
        Guid.Empty, [], new DrawnByResponse("Oğuzhan", "Ünsal"), 8, DateTime.UnixEpoch);

    private static ValidationBehavior<CreateDrawCommand, Result<DrawResponse>> Behavior() =>
        new([new CreateDrawCommandValidator()]);
}
