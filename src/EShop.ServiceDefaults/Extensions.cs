using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Polly;

namespace EShop.ServiceDefaults;

public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler(options =>
            {
                var resilience = builder.Configuration.GetSection("Resilience:Http");

                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(
                    resilience.GetValue("TotalRequestTimeoutSeconds", 30));
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(
                    resilience.GetValue("AttemptTimeoutSeconds", 10));

                options.Retry.MaxRetryAttempts = resilience.GetValue("Retry:MaxRetryAttempts", 3);
                options.Retry.Delay = TimeSpan.FromSeconds(resilience.GetValue("Retry:DelaySeconds", 1));
                options.Retry.MaxDelay = TimeSpan.FromSeconds(resilience.GetValue("Retry:MaxDelaySeconds", 15));
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.Retry.UseJitter = resilience.GetValue("Retry:UseJitter", true);
                options.Retry.ShouldHandle = args =>
                {
                    if (args.Outcome.Result?.RequestMessage is { } request &&
                        !IsSafeToRetry(request))
                    {
                        return new ValueTask<bool>(false);
                    }

                    return HttpClientResiliencePredicates.IsTransient(args.Outcome)
                        ? new ValueTask<bool>(true)
                        : new ValueTask<bool>(false);
                };

                options.CircuitBreaker.FailureRatio = resilience.GetValue("CircuitBreaker:FailureRatio", 0.5);
                options.CircuitBreaker.MinimumThroughput =
                    resilience.GetValue("CircuitBreaker:MinimumThroughput", 10);
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(
                    resilience.GetValue("CircuitBreaker:SamplingDurationSeconds", 30));
                options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(
                    resilience.GetValue("CircuitBreaker:BreakDurationSeconds", 30));
            });
            http.AddServiceDiscovery();
        });

        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live", "ready"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("live")
            })
            .AllowAnonymous();

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("ready") ||
                                            !registration.Tags.Contains("live")
            })
            .AllowAnonymous();

        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks("/healthz")
                .AllowAnonymous();
            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("live")
            })
            .AllowAnonymous();
        }

        return app;
    }

    private static bool IsSafeToRetry(HttpRequestMessage request)
    {
        return request.Method == HttpMethod.Get ||
               request.Method == HttpMethod.Head ||
               request.Method == HttpMethod.Options ||
               request.Method == HttpMethod.Trace ||
               request.Method == HttpMethod.Put ||
               request.Method == HttpMethod.Delete ||
               request.Headers.Contains("Idempotency-Key");
    }

    private static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter());
            builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());
        }

        return builder;
    }
}
