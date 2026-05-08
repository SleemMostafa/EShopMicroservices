namespace Identity.API.Data;

public static class IdentityDatabaseInitializer
{
    public static async Task InitializeIdentityDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IdentityDbContext>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        try
        {
            await dbContext.Database.EnsureCreatedAsync();
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Identity database initialization failed. The API will keep running, but identity endpoints that use the database may fail until SQL Server is available.");
        }
    }
}
