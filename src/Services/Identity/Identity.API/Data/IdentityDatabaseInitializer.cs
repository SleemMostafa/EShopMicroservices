namespace Identity.API.Data;

public static class IdentityDatabaseInitializer
{
    private const int MaxAttempts = 12;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    public static async Task InitializeIdentityDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IdentityDbContext>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await dbContext.Database.EnsureCreatedAsync();
                logger.LogInformation("Identity database initialized successfully.");
                return;
            }
            catch (Exception exception) when (attempt < MaxAttempts)
            {
                logger.LogWarning(
                    exception,
                    "Identity database initialization attempt {Attempt}/{MaxAttempts} failed. Retrying in {RetryDelaySeconds} seconds.",
                    attempt,
                    MaxAttempts,
                    RetryDelay.TotalSeconds);

                await Task.Delay(RetryDelay);
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Identity database initialization failed after {MaxAttempts} attempts. The API will keep running, but identity endpoints that use the database may fail until SQL Server is available.",
                    MaxAttempts);
            }
        }
    }
}
