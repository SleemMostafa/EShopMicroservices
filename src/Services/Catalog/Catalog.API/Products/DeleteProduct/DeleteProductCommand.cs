namespace Catalog.API.Products.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : ICommand<DeleteProductResponse>;

public sealed record DeleteProductResult(bool IsSuccess);

internal sealed class DeleteProductCommandHandler(
    IDocumentSession session,
    ILogger<DeleteProductCommandHandler> logger)
    : ICommandHandler<DeleteProductCommand, DeleteProductResponse>
{
    public async Task<DeleteProductResponse> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await session.LoadAsync<Product>(command.Id, cancellationToken);
        if (product is null)
        {
            throw new ProductNotFoundException(command.Id);
        }

        session.Delete<Product>(command.Id);

        await session.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Delete product successfully {id}", command.Id);

        return new DeleteProductResponse(true);
    }
}
