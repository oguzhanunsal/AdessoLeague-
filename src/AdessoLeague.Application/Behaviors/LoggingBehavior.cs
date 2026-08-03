using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdessoLeague.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var requestName = typeof(TRequest).Name;
        var startedAt = Stopwatch.GetTimestamp();

        logger.LogInformation("Handling {Request}", requestName);

        var response = await next(cancellationToken);
        var elapsedMs = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;

        if (response.IsSuccess)
        {
            logger.LogInformation("{Request} succeeded in {ElapsedMs} ms", requestName, elapsedMs);
        }
        else
        {
            logger.LogWarning(
                "{Request} failed in {ElapsedMs} ms with {ErrorCode}: {ErrorMessage}",
                requestName,
                elapsedMs,
                response.Error.Code,
                response.Error.Message);
        }

        return response;
    }
}
