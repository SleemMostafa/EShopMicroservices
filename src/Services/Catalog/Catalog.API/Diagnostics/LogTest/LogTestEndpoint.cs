namespace Catalog.API.Diagnostics.LogTest;

public sealed class LogTestEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/diagnostics/log-test", (ILogger<LogTestEndpoint> logger) =>
            {
                var correlationId = Guid.NewGuid();

                logger.LogTrace("Catalog log test trace event {CorrelationId}", correlationId);
                logger.LogDebug("Catalog log test debug event {CorrelationId}", correlationId);
                logger.LogInformation("Catalog log test information event {CorrelationId}", correlationId);
                logger.LogWarning("Catalog log test warning event {CorrelationId}", correlationId);
                logger.LogError("Catalog log test error event {CorrelationId}", correlationId);

                return Results.Ok(new
                {
                    Message = "Catalog log test events emitted.",
                    CorrelationId = correlationId
                });
            })
            .WithName("CatalogLogTest")
            .Produces(StatusCodes.Status200OK)
            .WithSummary("Emit Catalog log test events")
            .WithDescription("Emit Catalog log test events");
    }
}
