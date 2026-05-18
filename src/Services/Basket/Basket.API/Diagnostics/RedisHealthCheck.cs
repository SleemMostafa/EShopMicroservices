using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Basket.API.Diagnostics;

public sealed class RedisHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = configuration.GetConnectionString("Redis");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return HealthCheckResult.Unhealthy("Redis connection string is not configured.");
            }

            await using var connection = await ConnectionMultiplexer.ConnectAsync(connectionString);
            await connection.GetDatabase().PingAsync();

            return HealthCheckResult.Healthy("Basket Redis is healthy.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Basket Redis is unhealthy.", exception);
        }
    }
}
