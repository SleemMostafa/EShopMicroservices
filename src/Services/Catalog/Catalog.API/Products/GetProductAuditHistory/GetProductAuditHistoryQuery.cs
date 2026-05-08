namespace Catalog.API.Products.GetProductAuditHistory;

public sealed record GetProductAuditHistoryQuery(Guid ProductId) : IQuery<GetProductAuditHistoryResult>;

public sealed record GetProductAuditHistoryResult(IReadOnlyList<ProductAuditTrail> AuditHistory);

internal sealed class GetProductAuditHistoryQueryHandler(
    IDocumentSession session,
    ILogger<GetProductAuditHistoryQueryHandler> logger)
    : IQueryHandler<GetProductAuditHistoryQuery, GetProductAuditHistoryResult>
{
    public async Task<GetProductAuditHistoryResult> Handle(
        GetProductAuditHistoryQuery query,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("GetProductAuditHistoryQueryHandler.Handle called with query {@query}", query);

        var auditHistory = await session.Query<ProductAuditTrail>()
            .Where(auditTrail => auditTrail.ProductId == query.ProductId)
            .OrderByDescending(auditTrail => auditTrail.OccurredOn)
            .ToListAsync(cancellationToken);

        if (auditHistory.Count == 0)
        {
            var product = await session.LoadAsync<Product>(query.ProductId, cancellationToken);
            if (product is null)
            {
                throw new ProductNotFoundException("Product not found");
            }
        }

        return new GetProductAuditHistoryResult(auditHistory);
    }
}
