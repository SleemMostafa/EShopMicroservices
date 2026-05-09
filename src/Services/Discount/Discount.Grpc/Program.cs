using BuildingBlocks;
using BuildingBlocks.Authentication;
using BuildingBlocks.Identity;
using BuildingBlocks.Logging;
using BuildingBlocks.OpenApi;
using BuildingBlocks.Security;
using Discount.Grpc.Data;
using Discount.Grpc.Services;
using Microsoft.EntityFrameworkCore;

EshopSerilog.ConfigureBootstrapLogger(ApplicationNames.DiscountGrpc);

try
{
    EshopSerilog.LogStarting(ApplicationNames.DiscountGrpc);

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseEshopSerilog(ApplicationNames.DiscountGrpc);

    // Add services to the container.
    builder.Services.AddEshopOpenApi();
    builder.Services.AddGrpc();
    builder.Services.AddEshopDataProtection(builder.Configuration, ApplicationNames.DiscountGrpc);
    builder.Services.AddCurrentUserProvider();
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddDbContext<DiscountContext>(opts =>
        opts.UseSqlite(builder.Configuration.GetConnectionString("Database")));
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseEshopSerilogRequestLogging();
    app.UseJwtAuthentication();
    app.UseEshopOpenApi(ApplicationNames.DiscountGrpc);
    app.MapGrpcService<DiscountService>();
    app.UseMigration();
    app.MapGet("/",
        () =>
            "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

    app.Run();
}
catch (Exception exception)
{
    EshopSerilog.LogFatal(exception, ApplicationNames.DiscountGrpc);
}
finally
{
    await EshopSerilog.CloseAndFlushAsync();
}
