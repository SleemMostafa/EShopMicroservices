using BuildingBlocks.Logging;
using Discount.Grpc.Data;
using Discount.Grpc.Services;
using Microsoft.EntityFrameworkCore;

const string ApplicationName = "Discount.Grpc";

EshopSerilog.ConfigureBootstrapLogger(ApplicationName);

try
{
    EshopSerilog.LogStarting(ApplicationName);

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseEshopSerilog(ApplicationName);

    // Add services to the container.
    builder.Services.AddGrpc();
    builder.Services.AddDbContext<DiscountContext>(opts =>
        opts.UseSqlite(builder.Configuration.GetConnectionString("Database")));
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseEshopSerilogRequestLogging();
    app.MapGrpcService<DiscountService>();
    app.UseMigration();
    app.MapGet("/",
        () =>
            "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

    app.Run();
}
catch (Exception exception)
{
    EshopSerilog.LogFatal(exception, ApplicationName);
}
finally
{
    await EshopSerilog.CloseAndFlushAsync();
}
