using BuildingBlocks.Authentication;
using BuildingBlocks;
using BuildingBlocks.Logging;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

EshopSerilog.ConfigureBootstrapLogger(ApplicationNames.YarpApiGateway);

try
{
    EshopSerilog.LogStarting(ApplicationNames.YarpApiGateway);

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseEshopSerilog(ApplicationNames.YarpApiGateway);
    builder.AddServiceDefaults();
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddSlidingWindowLimiter("gateway-sliding-window", limiterOptions =>
        {
            limiterOptions.PermitLimit = builder.Configuration.GetValue("RateLimiting:SlidingWindow:PermitLimit", 100);
            limiterOptions.Window = TimeSpan.FromSeconds(
                builder.Configuration.GetValue("RateLimiting:SlidingWindow:WindowSeconds", 60));
            limiterOptions.SegmentsPerWindow =
                builder.Configuration.GetValue("RateLimiting:SlidingWindow:SegmentsPerWindow", 6);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = builder.Configuration.GetValue("RateLimiting:SlidingWindow:QueueLimit", 0);
        });

        options.AddFixedWindowLimiter("auth-fixed-window", limiterOptions =>
        {
            limiterOptions.PermitLimit = builder.Configuration.GetValue("RateLimiting:AuthFixedWindow:PermitLimit", 5);
            limiterOptions.Window = TimeSpan.FromSeconds(
                builder.Configuration.GetValue("RateLimiting:AuthFixedWindow:WindowSeconds", 60));
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = builder.Configuration.GetValue("RateLimiting:AuthFixedWindow:QueueLimit", 0);
        });

        options.AddFixedWindowLimiter("checkout-fixed-window", limiterOptions =>
        {
            limiterOptions.PermitLimit = builder.Configuration.GetValue("RateLimiting:CheckoutFixedWindow:PermitLimit", 10);
            limiterOptions.Window = TimeSpan.FromSeconds(
                builder.Configuration.GetValue("RateLimiting:CheckoutFixedWindow:WindowSeconds", 60));
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = builder.Configuration.GetValue("RateLimiting:CheckoutFixedWindow:QueueLimit", 0);
        });
    });
    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    var app = builder.Build();

    app.UseEshopSerilogRequestLogging();
    app.Use(async (context, next) =>
    {
        context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
        context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
        context.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
        context.Response.Headers.TryAdd("X-Permitted-Cross-Domain-Policies", "none");

        await next();
    });
    app.UseJwtAuthentication();
    app.UseRateLimiter();
    app.MapDefaultEndpoints();
    app.MapGet("/", () => Results.Ok(new
    {
        Application = ApplicationNames.YarpApiGateway,
        Status = "Running"
    }));
    app.MapReverseProxy();

    app.Run();
}
catch (Exception exception)
{
    EshopSerilog.LogFatal(exception, ApplicationNames.YarpApiGateway);
}
finally
{
    await EshopSerilog.CloseAndFlushAsync();
}
