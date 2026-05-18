using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace BuildingBlocks.Resilience;

public sealed class CorrelationIdMiddleware(
    RequestDelegate next,
    IOptions<CorrelationIdOptions> options)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headerName = options.Value.HeaderName;
        var correlationId = GetOrCreateCorrelationId(context, headerName);

        context.TraceIdentifier = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[headerName] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context, string headerName)
    {
        if (context.Request.Headers.TryGetValue(headerName, out var values) &&
            !string.IsNullOrWhiteSpace(values.FirstOrDefault()))
        {
            return values.First()!;
        }

        return context.TraceIdentifier;
    }
}
