using System.Reflection;
using FluentValidation;
using MediatR;

namespace AdessoLeague.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private static readonly Func<Error, TResponse> ToFailure = BuildFailureFactory();

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(validators
            .Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .Select(failure => Error.Validation(failure.PropertyName, failure.ErrorMessage))
            .ToList();

        return failures.Count == 0
            ? await next(cancellationToken)
            : ToFailure(new ValidationError(failures));
    }

    // TResponse is either Result or Result<T>; the generic factory can only be reached reflectively.
    private static Func<Error, TResponse> BuildFailureFactory()
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return error => (TResponse)Result.Failure(error);
        }

        var failure = typeof(Result)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method => method.Name == nameof(Result.Failure) && method.IsGenericMethodDefinition)
            .MakeGenericMethod(typeof(TResponse).GetGenericArguments()[0]);

        return error => (TResponse)failure.Invoke(null, [error])!;
    }
}
