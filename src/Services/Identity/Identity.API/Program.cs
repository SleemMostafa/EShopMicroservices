using BuildingBlocks.Logging;
using Identity.API;

EshopSerilog.ConfigureBootstrapLogger(ApplicationNames.IdentityApi);

try
{
    EshopSerilog.LogStarting(ApplicationNames.IdentityApi);

    var builder = WebApplication.CreateBuilder(args);

    builder.AddIdentityServices(ApplicationNames.IdentityApi);

    var app = builder.Build();

    await app.UseIdentityServices(ApplicationNames.IdentityApi);

    app.Run();
}
catch (Exception exception)
{
    EshopSerilog.LogFatal(exception, ApplicationNames.IdentityApi);
}
finally
{
    await EshopSerilog.CloseAndFlushAsync();
}
