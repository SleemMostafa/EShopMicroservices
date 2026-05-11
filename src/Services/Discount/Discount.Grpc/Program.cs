using BuildingBlocks.Logging;
using Discount.Grpc;

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
    EshopSerilog.LogFatal(exception, ApplicationNames.DiscountGrpc);
}
finally
{
    await EshopSerilog.CloseAndFlushAsync();
}
