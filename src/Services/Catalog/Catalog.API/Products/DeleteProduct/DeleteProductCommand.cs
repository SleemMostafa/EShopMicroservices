namespace Catalog.API.Products.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : ICommand<DeleteProductResponse>;

public sealed record DeleteProductResult(bool IsSuccess);

internal sealed class DeleteProductCommandHandler
    : ICommandHandler<DeleteProductCommand, DeleteProductResponse>
{
    private readonly IProductRepository _repository;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(IProductRepository repository, ILogger<DeleteProductCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<DeleteProductResponse> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
        {
            throw new ProductNotFoundException(command.Id);
        }

        await _repository.DeleteAsync(command.Id, cancellationToken);
        _logger.LogInformation("Delete product successfully {id}", command.Id);

        return new DeleteProductResponse(true);
    }
}
