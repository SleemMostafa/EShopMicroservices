using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Logging;
using BuildingBlocks.Resilience;
using EShop.ServiceDefaults;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Ordering.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCarter();

        services.AddExceptionHandler<CustomExceptionHandler>();
        services.AddHealthChecks()
            .AddSqlServer(configuration.GetConnectionString("Database")!);

        return services;
    }

    public static WebApplication UseApiServices(this WebApplication app)
    {
        app.UseEshopCorrelationId();
        app.UseEshopSerilogRequestLogging();
        app.UseExceptionHandler(options => { });
        app.MapDefaultEndpoints();
        app.UseHealthChecks("/health",
            new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        app.MapCarter();

        return app;
    }
}
