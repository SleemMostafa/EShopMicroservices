namespace Catalog.API.Products.GetProducts;

public sealed record GetProductsQuery() : IQuery<GetProductsResult>;

public record GetProductsResult(IEnumerable<Product> Products);

internal sealed class GetProductsQueryHandler 
    : IQueryHandler<GetProductsQuery, GetProductsResult>
{
    private readonly IDocumentSession _session;
    private readonly ILogger<GetProductsQueryHandler> _logger;

    public GetProductsQueryHandler(IDocumentSession session, ILogger<GetProductsQueryHandler> logger)
    {
        _session = session;
        _logger = logger;
    }

    public async Task<GetProductsResult> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetProductsQueryHandler.Handle called with {@Query}",query);

        var products = await _session.Query<Product>()
            .ToListAsync(cancellationToken);

        return new GetProductsResult(products);
    }
}