namespace Catalog.API.Products.GetProductByCategory;

public sealed record GetProductsByCategoryQuery(string Category) : IQuery<GetProductsByCategoryResult>;

public sealed record GetProductsByCategoryResult(IReadOnlyList<Product> Products);

internal sealed class GetProductsByCategoryQueryHandler(
    IDocumentSession session,
    ILogger<GetProductsByCategoryQueryHandler> logger)
    : IQueryHandler<GetProductsByCategoryQuery, GetProductsByCategoryResult>
{
    public async Task<GetProductsByCategoryResult> Handle(GetProductsByCategoryQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("GetProductsByCategoryQueryHandler.Handle called with query {@query}", query);

        var products = await session.Query<Product>()
            .Where(product => product.Category.Contains(query.Category))
            .ToListAsync(cancellationToken);

        return new GetProductsByCategoryResult(products);
    }
}
