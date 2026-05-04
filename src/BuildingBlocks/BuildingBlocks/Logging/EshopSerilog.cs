using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace BuildingBlocks.Logging;

public static class EshopSerilog
{
    private const string ConsoleOutputTemplate =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";

    public static void ConfigureBootstrapLogger(string applicationName)
    {
        Log.Logger = CreateLoggerConfiguration(applicationName)
            .WriteToSeqIfConfigured(
                Environment.GetEnvironmentVariable("Seq__ServerUrl") ??
                Environment.GetEnvironmentVariable("SEQ_SERVER_URL"),
                Environment.GetEnvironmentVariable("Seq__ApiKey") ??
                Environment.GetEnvironmentVariable("SEQ_API_KEY"))
            .CreateBootstrapLogger();
    }

    public static IHostBuilder UseEshopSerilog(this IHostBuilder hostBuilder, string applicationName)
    {
        return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            CreateLoggerConfiguration(applicationName, loggerConfiguration)
                .WriteToSeqIfConfigured(
                    context.Configuration["Seq:ServerUrl"],
                    context.Configuration["Seq:ApiKey"])
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services);
        });
    }

    public static IApplicationBuilder UseEshopSerilogRequestLogging(this IApplicationBuilder app)
    {
        return app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "Handled {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            options.GetLevel = (httpContext, _, exception) =>
                exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError
                    ? LogEventLevel.Error
                    : LogEventLevel.Information;

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            };
        });
    }

    public static void LogStarting(string applicationName)
    {
        Log.Information("Starting {ApplicationName}", applicationName);
    }

    public static void LogFatal(Exception exception, string applicationName)
    {
        Log.Fatal(exception, "{ApplicationName} terminated unexpectedly", applicationName);
    }

    public static ValueTask CloseAndFlushAsync()
    {
        return Log.CloseAndFlushAsync();
    }

    private static LoggerConfiguration CreateLoggerConfiguration(string applicationName)
    {
        return CreateLoggerConfiguration(applicationName, new LoggerConfiguration());
    }

    private static LoggerConfiguration CreateLoggerConfiguration(
        string applicationName,
        LoggerConfiguration loggerConfiguration)
    {
        return loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", applicationName)
            .WriteTo.Console(outputTemplate: ConsoleOutputTemplate);
    }

    private static LoggerConfiguration WriteToSeqIfConfigured(
        this LoggerConfiguration loggerConfiguration,
        string? serverUrl,
        string? apiKey)
    {
        return string.IsNullOrWhiteSpace(serverUrl)
            ? loggerConfiguration
            : loggerConfiguration.WriteTo.Seq(serverUrl, apiKey: apiKey);
    }
}
