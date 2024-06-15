namespace Catalog.API.Products.GetProductByCategory;

public sealed record GetProductsByCategoryQuery(string Category):IQuery<GetProductsByCategoryResult>;
public sealed record GetProductsByCategoryResult(IEnumerable<ProductDto> Product);

internal sealed class GetProductsByCategoryQueryHandler
    : IQueryHandler<GetProductsByCategoryQuery, GetProductsByCategoryResult>
{
    private readonly IDocumentSession _session;
    private readonly ILogger<GetProductsByCategoryQueryHandler> _logger;

    public GetProductsByCategoryQueryHandler(IDocumentSession session, ILogger<GetProductsByCategoryQueryHandler> logger)
    {
        _session = session;
        _logger = logger;
    }

    public async Task<GetProductsByCategoryResult> Handle(GetProductsByCategoryQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetProductsByCategoryQueryHandler.Handle called with query {@query}", query);

        var products = await _session.Query<Product>()
            .Where(product => product.Category.Contains(query.Category))
            .ToListAsync(cancellationToken);

        return new GetProductsByCategoryResult(products.Adapt<IEnumerable<ProductDto>>());
    }
}