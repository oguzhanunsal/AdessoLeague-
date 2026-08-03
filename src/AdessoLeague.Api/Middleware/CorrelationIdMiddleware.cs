using System.Globalization;
using Serilog.Context;

namespace AdessoLeague.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var supplied)
            && !string.IsNullOrWhiteSpace(supplied)
                ? supplied.ToString()
                : Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);

        context.TraceIdentifier = correlationId;

        // Set on send rather than now: the exception handler clears the response, and the header
        // matters most on the 500 it then writes.
        context.Response.OnStarting(state =>
        {
            ((HttpContext)state).Response.Headers[HeaderName] = correlationId;

            return Task.CompletedTask;
        }, context);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
