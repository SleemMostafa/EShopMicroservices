namespace Catalog.API.Products.GetProductByCategory;

public sealed record GetProductsByCategoryQuery(string Category) : IQuery<GetProductsByCategoryResult>;

public sealed record GetProductsByCategoryResult(IEnumerable<Product> Products);

internal sealed class GetProductsByCategoryQueryHandler
    : IQueryHandler<GetProductsByCategoryQuery, GetProductsByCategoryResult>
{
    private readonly IProductRepository _repository;
    private readonly ILogger<GetProductsByCategoryQueryHandler> _logger;

    public GetProductsByCategoryQueryHandler(
        IProductRepository repository,
        ILogger<GetProductsByCategoryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<GetProductsByCategoryResult> Handle(GetProductsByCategoryQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetProductsByCategoryQueryHandler.Handle called with query {@query}", query);

        var products = await _repository.GetByCategoryAsync(query.Category, cancellationToken);

        return new GetProductsByCategoryResult(products);
    }
}
