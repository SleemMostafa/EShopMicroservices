using Discount.Grpc;
using BuildingBlocks.Authentication;
using BuildingBlocks.Identity;
using BuildingBlocks.Logging;
using BuildingBlocks.OpenApi;

const string ApplicationName = "Basket.API";

EshopSerilog.ConfigureBootstrapLogger(ApplicationName);

try
{
    EshopSerilog.LogStarting(ApplicationName);

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseEshopSerilog(ApplicationName);

    // Add services to the container.

    //Application Services
    var assembly = typeof(Program).Assembly;
    builder.Services.AddEshopOpenApi();
    builder.Services.AddCarter();
    builder.Services.AddMediatR(config =>
    {
        config.RegisterServicesFromAssembly(assembly);
        config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        config.AddOpenBehavior(typeof(LoggingBehavior<,>));
    });

    //Data Services
    builder.Services.AddMarten(opts =>
    {
        opts.Connection(builder.Configuration.GetConnectionString("Database")!);
        opts.Schema.For<ShoppingCart>().Identity(x => x.UserName);
    }).UseLightweightSessions();

    builder.Services.AddScoped<IBasketRepository, BasketRepository>();
    builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
        //options.InstanceName = "Basket";
    });

    //Grpc Services
    builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
    {
        options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!);
    });

    //Cross-Cutting Services
    builder.Services.AddCurrentUserProvider();
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddExceptionHandler<CustomExceptionHandler>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseEshopSerilogRequestLogging();
    app.UseExceptionHandler(options => { });
    app.UseJwtAuthentication();
    app.UseEshopOpenApi(ApplicationName);
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
