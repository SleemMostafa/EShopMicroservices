using BuildingBlocks.Logging;

const string ApplicationName = "Identity.API";

EshopSerilog.ConfigureBootstrapLogger(ApplicationName);

try
{
    EshopSerilog.LogStarting(ApplicationName);

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseEshopSerilog(ApplicationName);

    var assembly = typeof(Program).Assembly;

    builder.Services.AddEshopOpenApi();
    builder.Services.AddCarter();
    builder.Services.AddMediatR(config =>
    {
        config.RegisterServicesFromAssembly(assembly);
        config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        config.AddOpenBehavior(typeof(LoggingBehavior<,>));
    });
    builder.Services.AddValidatorsFromAssembly(assembly);
    builder.Services.AddExceptionHandler<CustomExceptionHandler>();
    builder.Services.AddSingleton<IEshopClock, EshopClock>();
    builder.Services.AddCurrentUserProvider();
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

    builder.Services.AddDbContext<IdentityDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

    var app = builder.Build();

    app.UseEshopSerilogRequestLogging();
    app.UseExceptionHandler(options => { });
    app.UseJwtAuthentication();
    app.UseEshopOpenApi(ApplicationName);
    app.MapCarter();
    await app.InitializeIdentityDatabaseAsync();

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
