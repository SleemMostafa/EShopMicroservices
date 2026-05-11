using Discount.Grpc.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Discount.Grpc.Diagnostics;

public sealed class SqliteHealthCheck(DiscountContext context) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthCheckContext,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await context.Database.CanConnectAsync(cancellationToken);

        return canConnect
            ? HealthCheckResult.Healthy("Discount database is healthy.")
            : HealthCheckResult.Unhealthy("Discount database is unavailable.");
    }
}
