using BuildingBlocks;
using BuildingBlocks.Logging;
using BuildingBlocks.Resilience;
using EShop.ServiceDefaults;
using Ordering.API;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Data.Extensions;

EshopSerilog.ConfigureBootstrapLogger(ApplicationNames.OrderingApi);

try
{
    EshopSerilog.LogStarting(ApplicationNames.OrderingApi);

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseEshopSerilog(ApplicationNames.OrderingApi);
    builder.AddServiceDefaults();
    builder.Services.AddEshopCorrelationId(builder.Configuration);
    builder.Services.AddEshopIdempotency(builder.Configuration);

    builder.Services
        .AddApplicationServices(builder.Configuration)
        .AddInfrastructureServices(builder.Configuration)
        .AddApiServices(builder.Configuration);

    var app = builder.Build();

    app.UseApiServices();

    if (app.Environment.IsDevelopment())
    {
        await app.InitialiseDatabaseAsync();
    }

    app.Run();
}
catch (Exception exception)
{
    EshopSerilog.LogFatal(exception, ApplicationNames.OrderingApi);
}
finally
{
    await EshopSerilog.CloseAndFlushAsync();
}
