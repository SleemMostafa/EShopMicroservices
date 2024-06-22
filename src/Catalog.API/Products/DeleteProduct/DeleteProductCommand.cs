namespace Catalog.API.Products.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : ICommand<DeleteProductResponse>;

public sealed record DeleteProductResult(bool IsSuccess);

internal sealed class DeleteProductCommandHandler
    : ICommandHandler<DeleteProductCommand, DeleteProductResponse>
{
    private readonly IDocumentSession _session;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(IDocumentSession session, ILogger<DeleteProductCommandHandler> logger)
    {
        _session = session;
        _logger = logger;
    }

    public async Task<DeleteProductResponse> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _session.Delete<Product>(command.Id);

            await _session.SaveChangesAsync(cancellationToken);
            return new DeleteProductResponse(true);
        }
        catch (Exception e)
        {
            _logger.LogError("Something went wrong on delete product {id}", command.Id);
            return new DeleteProductResponse(false);
        }
    }
}