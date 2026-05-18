using BuildingBlocks.Authentication;
using BuildingBlocks.Logging;
using BuildingBlocks.OpenApi;
using BuildingBlocks.Resilience;
using BuildingBlocks.Security;
using BuildingBlocks.Messaging.MassTransit;
using Basket.API.Infrastructure.Grpc;
using Basket.API.Diagnostics;
using Discount.Grpc;
using EShop.ServiceDefaults;
using Grpc.Net.ClientFactory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Basket.API;

public static class Setup
{
    public static WebApplicationBuilder AddBasketServices(
        this WebApplicationBuilder builder,
        string applicationName)
    {
        builder.Host.UseEshopSerilog(applicationName);
        builder.AddServiceDefaults();
        builder.Services.AddEshopCorrelationId(builder.Configuration);

        var assembly = typeof(Setup).Assembly;

        builder.Services.AddEshopOpenApi();
        builder.Services.AddCarter();
        builder.Services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });
        builder.Services.AddEshopIdempotency(builder.Configuration);
        builder.Services.AddValidatorsFromAssembly(assembly);

        builder.Services.AddMarten(options =>
            {
                options.Connection(builder.Configuration.GetConnectionString("Database")!);
                options.UseNewtonsoftForSerialization(nonPublicMembersStorage: NonPublicMembersStorage.NonPublicSetters);
            })
            .UseLightweightSessions();

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("Redis");
        });
        builder.Services.AddScoped<ICacheService, DistributedCacheService>();

        builder.Services.AddSingleton<IEshopClock, EshopClock>();
        builder.Services.AddInternalServiceTokenProvider(builder.Configuration);
        builder.Services.AddTransient<InternalServiceAuthInterceptor>();
        builder.Services
            .AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
                options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!))
            .AddInterceptor<InternalServiceAuthInterceptor>(InterceptorScope.Client);

        builder.Services.AddEshopDataProtection(builder.Configuration, applicationName);
        builder.Services.AddCurrentUserProvider();
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
        builder.Services.AddExceptionHandler<CustomExceptionHandler>();
        builder.Services.AddMessageBroker(builder.Configuration);
        builder.Services.AddHealthChecks()
            .AddCheck("basket-api", () => HealthCheckResult.Healthy("Basket API is healthy."), ["ready"])
            .AddCheck<PostgresHealthCheck>("basket-postgres", tags: ["ready"])
            .AddCheck<RedisHealthCheck>("basket-redis", tags: ["ready"]);

        return builder;
    }

    public static WebApplication UseBasketServices(
        this WebApplication app,
        string applicationName)
    {
        app.UseEshopCorrelationId();
        app.UseEshopSerilogRequestLogging();
        app.UseExceptionHandler(_ => { });
        app.UseJwtAuthentication();
        app.UseEshopOpenApi(applicationName);
        app.MapDefaultEndpoints();
        app.MapCarter();

        return app;
    }
}
