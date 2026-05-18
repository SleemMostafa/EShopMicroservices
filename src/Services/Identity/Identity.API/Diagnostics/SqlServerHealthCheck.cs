using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Identity.API.Diagnostics;

public sealed class SqlServerHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(configuration.GetConnectionString("Database"));
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("Identity SQL Server is healthy.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Identity SQL Server is unhealthy.", exception);
        }
    }
}
