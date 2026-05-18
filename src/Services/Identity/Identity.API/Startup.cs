using BuildingBlocks.Logging;
using BuildingBlocks.Resilience;
using BuildingBlocks.Security;
using EShop.ServiceDefaults;
using Identity.API.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Identity.API;

public static class Startup
{
    public static WebApplicationBuilder AddIdentityServices(
        this WebApplicationBuilder builder,
        string applicationName)
    {
        builder.Host.UseEshopSerilog(applicationName);
        builder.AddServiceDefaults();
        builder.Services.AddEshopCorrelationId(builder.Configuration);

        var assembly = typeof(Startup).Assembly;

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
        builder.Services.AddExceptionHandler<CustomExceptionHandler>();
        builder.Services.AddEshopDataProtection(builder.Configuration, applicationName);
        builder.Services.AddSingleton<IEshopClock, EshopClock>();
        builder.Services.AddCurrentUserProvider();
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
        builder.Services.AddHealthChecks()
            .AddCheck("identity-api", () => HealthCheckResult.Healthy("Identity API is healthy."), ["ready"])
            .AddCheck<SqlServerHealthCheck>("identity-sqlserver", tags: ["ready"]);

        builder.Services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

        return builder;
    }

    public static async Task<WebApplication> UseIdentityServices(
        this WebApplication app,
        string applicationName)
    {
        app.UseEshopCorrelationId();
        app.UseEshopSerilogRequestLogging();
        app.UseExceptionHandler(options => { });
        app.UseJwtAuthentication();
        app.UseEshopOpenApi(applicationName);
        app.MapDefaultEndpoints();
        app.MapCarter();

        await app.InitializeIdentityDatabaseAsync();

        return app;
    }
}
