using Catalog.API;
using BuildingBlocks.Logging;

EshopSerilog.ConfigureBootstrapLogger(ApplicationNames.CatalogApi);

try
{
    EshopSerilog.LogStarting(ApplicationNames.CatalogApi);

    var builder = WebApplication.CreateBuilder(args);

    builder.AddCatalogServices(ApplicationNames.CatalogApi);

    var app = builder.Build();

    app.UseCatalogServices(ApplicationNames.CatalogApi);

    app.Run();
}
catch (Exception exception)
{
    EshopSerilog.LogFatal(exception, ApplicationNames.CatalogApi);
}
finally
{
    await EshopSerilog.CloseAndFlushAsync();
}
