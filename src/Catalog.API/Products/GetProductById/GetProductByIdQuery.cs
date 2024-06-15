namespace Catalog.API.Products.GetProductById;

public sealed record GetProductByIdQuery(Guid Id):IQuery<GetProductByIdResult>;
public sealed record GetProductByIdResult(Product Product);

internal sealed class GetProductByIdQueryHandler
    : IQueryHandler<GetProductByIdQuery, GetProductByIdResult>
{
    private readonly IDocumentSession _session;
    private readonly ILogger<GetProductByIdQueryHandler> _logger;

    public GetProductByIdQueryHandler(IDocumentSession session, ILogger<GetProductByIdQueryHandler> logger)
    {
        _session = session;
        _logger = logger;
    }

    public async Task<GetProductByIdResult> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetProductByIdQueryHandler.Handle called with query {@query}",query);
        var product = await _session.LoadAsync<Product>(query.Id, cancellationToken);

        if (product is null)
        {
            _logger.LogInformation("Product related that {id} not exist",query.Id);
            
            throw new ProductNotFoundException();
        }
        return new GetProductByIdResult(product);
    }
}