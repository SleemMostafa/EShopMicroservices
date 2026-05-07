namespace Catalog.API.Products.GetProductAuditHistory;

public sealed record GetProductAuditHistoryResponse(IReadOnlyList<ProductAuditTrailDto> AuditHistory);

public sealed class GetProductAuditHistoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id}/audit-history", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetProductAuditHistoryQuery(id), ct);

                var response = ProductMapper.ToResponse(result);

                return Results.Ok(response);
            })
            .WithName("GetProductAuditHistory")
            .Produces<GetProductAuditHistoryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get product audit history")
            .WithDescription("Get product audit history");
    }
}
