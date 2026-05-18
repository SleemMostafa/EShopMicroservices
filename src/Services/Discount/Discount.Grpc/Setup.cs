using BuildingBlocks.Authentication;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Logging;
using BuildingBlocks.OpenApi;
using BuildingBlocks.Resilience;
using BuildingBlocks.Security;
using Discount.Grpc.Diagnostics;
using Discount.Grpc.Discounts;
using EShop.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Discount.Grpc;

public static class Setup
{
    public static WebApplicationBuilder AddDiscountServices(
        this WebApplicationBuilder builder,
        string applicationName)
    {
        builder.Host.UseEshopSerilog(applicationName);
        builder.AddServiceDefaults();
        builder.Services.AddEshopCorrelationId(builder.Configuration);

        builder.Services.AddEshopOpenApi();
        builder.Services.AddGrpc();
        builder.Services.AddEshopDataProtection(builder.Configuration, applicationName);
        builder.Services.AddCurrentUserProvider();
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddInternalServiceAuthorization(
            InternalServicePolicies.DiscountRead,
            ApplicationNames.BasketApi,
            "discount.read");
        builder.Services.AddExceptionHandler<CustomExceptionHandler>();
        builder.Services.AddDbContext<DiscountContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("Database")));
        builder.Services.AddHealthChecks()
            .AddCheck("discount-grpc", () => HealthCheckResult.Healthy("Discount gRPC is healthy."))
            .AddCheck<SqliteHealthCheck>("discount-sqlite");

        return builder;
    }

    public static async Task<WebApplication> UseDiscountServices(
        this WebApplication app,
        string applicationName)
    {
        app.UseEshopCorrelationId();
        app.UseEshopSerilogRequestLogging();
        app.UseExceptionHandler(_ => { });
        app.UseJwtAuthentication();
        app.UseEshopOpenApi(applicationName);
        app.MapDefaultEndpoints();
        app.MapHealthChecks("/health")
            .AllowAnonymous();
        app.MapGrpcService<DiscountService>()
            .RequireAuthorization(InternalServicePolicies.DiscountRead);
        app.MapGet("/",
            () =>
                "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        await app.ApplyMigrationsAsync();

        return app;
    }
}
