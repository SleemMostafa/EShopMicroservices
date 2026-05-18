using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Basket.API.Diagnostics;

public sealed class PostgresHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(configuration.GetConnectionString("Database"));
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("Basket PostgreSQL is healthy.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Basket PostgreSQL is unhealthy.", exception);
        }
    }
}
