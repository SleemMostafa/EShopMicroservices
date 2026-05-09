using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Catalog.API.Diagnostics;

public sealed class PostgresHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("Database");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return HealthCheckResult.Unhealthy("Catalog database connection string is not configured.");
        }

        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("Catalog PostgreSQL database is healthy.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Catalog PostgreSQL database is unhealthy.", exception);
        }
    }
}
