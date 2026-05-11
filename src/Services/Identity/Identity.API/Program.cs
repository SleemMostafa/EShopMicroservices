using BuildingBlocks.Logging;
using BuildingBlocks.Security;

EshopSerilog.ConfigureBootstrapLogger(ApplicationNames.IdentityApi);

try
{
    EshopSerilog.LogStarting(ApplicationNames.IdentityApi);

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseEshopSerilog(ApplicationNames.IdentityApi);
    builder.AddServiceDefaults();

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
    builder.Services.AddEshopDataProtection(builder.Configuration, ApplicationNames.IdentityApi);
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
    app.UseEshopOpenApi(ApplicationNames.IdentityApi);
    app.MapDefaultEndpoints();
    app.MapCarter();
    await app.InitializeIdentityDatabaseAsync();

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
