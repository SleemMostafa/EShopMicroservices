using BuildingBlocks.Authentication;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Logging;
using BuildingBlocks.OpenApi;
using BuildingBlocks.Security;
using Catalog.API.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Catalog.API;

public static class Setup
{
    public static WebApplicationBuilder AddCatalogServices(
        this WebApplicationBuilder builder,
        string applicationName)
    {
        builder.Host.UseEshopSerilog(applicationName);
        builder.AddServiceDefaults();

        var assembly = typeof(Setup).Assembly;

        builder.Services.AddEshopOpenApi();
        builder.Services.AddCarter();
        builder.Services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });
        builder.Services.AddValidatorsFromAssembly(assembly);

        builder.Services.AddSingleton<IEshopClock, EshopClock>();
        builder.Services.AddExceptionHandler<CustomExceptionHandler>();
        builder.Services.AddEshopDataProtection(builder.Configuration, applicationName);
        builder.Services.AddCurrentUserProvider();
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddHealthChecks()
            .AddCheck("catalog-api", () => HealthCheckResult.Healthy("Catalog API is healthy."))
            .AddCheck<PostgresHealthCheck>("catalog-postgres");

        builder.Services.AddMarten(config =>
            {
                config.Connection(builder.Configuration.GetConnectionString("Database")!);
                config.UseNewtonsoftForSerialization(nonPublicMembersStorage: NonPublicMembersStorage.NonPublicSetters);
            })
            .UseLightweightSessions();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.InitializeMartenWith<CatalogInitialData>();
        }

        return builder;
    }

    public static WebApplication UseCatalogServices(
        this WebApplication app,
        string applicationName)
    {
        app.UseEshopSerilogRequestLogging();
        app.UseExceptionHandler(_ => { });
        app.UseJwtAuthentication();
        app.UseEshopOpenApi(applicationName);
        app.MapDefaultEndpoints();
        app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = HealthCheckResponseWriter.WriteAsync
            })
            .AllowAnonymous();
        app.MapCarter();

        return app;
    }
}
