using BuildingBlocks.Logging;
using Discount.Grpc;
using Microsoft.Extensions.Hosting;

EshopSerilog.ConfigureBootstrapLogger(ApplicationNames.DiscountGrpc);

try
{
    EshopSerilog.LogStarting(ApplicationNames.DiscountGrpc);

    var builder = WebApplication.CreateBuilder(args);

    builder.AddDiscountServices(ApplicationNames.DiscountGrpc);

    var app = builder.Build();

    await app.UseDiscountServices(ApplicationNames.DiscountGrpc);

    app.Run();
}
catch (Exception exception)
{
    if (exception is HostAbortedException)
    {
        return;
    }

    EshopSerilog.LogFatal(exception, ApplicationNames.DiscountGrpc);
}
finally
{
    await EshopSerilog.CloseAndFlushAsync();
}
