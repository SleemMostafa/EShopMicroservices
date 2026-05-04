using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Logging;
using Catalog.API.Data;
using FluentValidation;

const string ApplicationName = "Catalog.API";

EshopSerilog.ConfigureBootstrapLogger(ApplicationName);

try
{
    EshopSerilog.LogStarting(ApplicationName);

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseEshopSerilog(ApplicationName);

    // Add services to the container.
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    var assembly = typeof(Program).Assembly;
    builder.Services.AddMediatR(config =>
    {
        config.RegisterServicesFromAssembly(assembly);
        config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        config.AddOpenBehavior(typeof(LoggingBehavior<,>));
    });
    builder.Services.AddValidatorsFromAssembly(assembly);
    builder.Services.AddSingleton<IEshopClock, EshopClock>();


    builder.Services.AddExceptionHandler<CustomExceptionHandler>();
    builder.Services.AddCarter();
    builder.Services.AddMarten(config => { config.Connection(builder.Configuration.GetConnectionString("Database")!); })
        .UseLightweightSessions();
    builder.Services.AddScoped<IProductRepository, MartenProductRepository>();

    if (builder.Environment.IsDevelopment())
    {
        builder.Services.InitializeMartenWith<CatalogInitialData>();
    }

    var app = builder.Build();

    app.UseEshopSerilogRequestLogging();
    app.UseExceptionHandler(option => { });
    app.MapCarter();

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
