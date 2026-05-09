namespace Catalog.API.Products.GetProducts;

public sealed record GetProductsQuery(PagingOptionsRequest PagingOptions) : IQuery<GetProductsResult>;

public sealed record GetProductsResult(PaginatedList<Product> Products);

internal sealed class GetProductsQueryHandler(IDocumentSession session, ILogger<GetProductsQueryHandler> logger)
    : IQueryHandler<GetProductsQuery, GetProductsResult>
{
    public async Task<GetProductsResult> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("GetProductsQueryHandler.Handle called with {@Query}", query);

        var products = await session.Query<Product>().ToPagedListAsync(
            query.PagingOptions,
            cancellationToken: cancellationToken);

        return new GetProductsResult(products);
    }
}
