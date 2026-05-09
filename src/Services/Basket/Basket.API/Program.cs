using BuildingBlocks;
using Discount.Grpc;
using BuildingBlocks.Authentication;
using BuildingBlocks.Identity;
using BuildingBlocks.Logging;
using BuildingBlocks.OpenApi;
using BuildingBlocks.Security;

EshopSerilog.ConfigureBootstrapLogger(ApplicationNames.BasketApi);

try
{
    EshopSerilog.LogStarting(ApplicationNames.BasketApi);

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseEshopSerilog(ApplicationNames.BasketApi);

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
    builder.Services.AddEshopDataProtection(builder.Configuration, ApplicationNames.BasketApi);
    builder.Services.AddCurrentUserProvider();
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddExceptionHandler<CustomExceptionHandler>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseEshopSerilogRequestLogging();
    app.UseExceptionHandler(options => { });
    app.UseJwtAuthentication();
    app.UseEshopOpenApi(ApplicationNames.BasketApi);
    app.MapCarter();

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
