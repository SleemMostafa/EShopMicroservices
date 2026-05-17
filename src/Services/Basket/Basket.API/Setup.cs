using BuildingBlocks.Authentication;
using BuildingBlocks.Logging;
using BuildingBlocks.OpenApi;
using BuildingBlocks.Security;
using Discount.Grpc;
using Grpc.Net.Client;

namespace Basket.API;

public static class Setup
{
    public static WebApplicationBuilder AddBasketServices(
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

        builder.Services.AddSingleton(_ =>
        {
            var discountUrl = builder.Configuration["GrpcSettings:DiscountUrl"]!;
            var channel = GrpcChannel.ForAddress(discountUrl);

            return new DiscountProtoService.DiscountProtoServiceClient(channel);
        });

        builder.Services.AddEshopDataProtection(builder.Configuration, applicationName);
        builder.Services.AddCurrentUserProvider();
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddExceptionHandler<CustomExceptionHandler>();

        return builder;
    }

    public static WebApplication UseBasketServices(
        this WebApplication app,
        string applicationName)
    {
        app.UseEshopSerilogRequestLogging();
        app.UseExceptionHandler(_ => { });
        app.UseJwtAuthentication();
        app.UseEshopOpenApi(applicationName);
        app.MapDefaultEndpoints();
        app.MapCarter();

        return app;
    }
}
