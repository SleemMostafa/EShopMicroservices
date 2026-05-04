namespace Catalog.API.Products.GetProducts;

public sealed record GetProductsQuery() : IQuery<GetProductsResult>;

public record GetProductsResult(IEnumerable<Product> Products);

internal sealed class GetProductsQueryHandler
    : IQueryHandler<GetProductsQuery, GetProductsResult>
{
    private readonly IProductRepository _repository;
    private readonly ILogger<GetProductsQueryHandler> _logger;

    public GetProductsQueryHandler(IProductRepository repository, ILogger<GetProductsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<GetProductsResult> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetProductsQueryHandler.Handle called with {@Query}", query);

        var products = await _repository.GetAllAsync(cancellationToken);

        return new GetProductsResult(products);
    }
}
