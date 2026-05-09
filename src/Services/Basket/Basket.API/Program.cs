using Basket.API;
using BuildingBlocks.Logging;

EshopSerilog.ConfigureBootstrapLogger(ApplicationNames.BasketApi);

try
{
    EshopSerilog.LogStarting(ApplicationNames.BasketApi);

    var builder = WebApplication.CreateBuilder(args);

    builder.AddBasketServices(ApplicationNames.BasketApi);

    var app = builder.Build();

    app.UseBasketServices(ApplicationNames.BasketApi);

    app.Run();
}
catch (Exception exception)
{
    EshopSerilog.LogFatal(exception, ApplicationNames.BasketApi);
}
finally
{
    await EshopSerilog.CloseAndFlushAsync();
}
