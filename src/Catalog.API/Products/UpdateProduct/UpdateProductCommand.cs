namespace Catalog.API.Products.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    string ImageFile,
    decimal Price,
    List<string> Category
) : ICommand<UpdateProductResult>;

public sealed record UpdateProductResult(bool IsSuccess);

internal sealed class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, UpdateProductResult>
{
    private readonly IDocumentSession _session;
    private readonly ILogger<UpdateProductCommandHandler> _logger;
    private readonly IEshopClock _clock;

    public UpdateProductCommandHandler(IDocumentSession session, ILogger<UpdateProductCommandHandler> logger, IEshopClock clock)
    {
        _session = session;
        _logger = logger;
        _clock = clock;
    }

    public async Task<UpdateProductResult> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("UpdateProductCommandHandler.Handle called with {command}", command);

        try
        {
            var product = await _session
                .LoadAsync<Product>(command.Id, cancellationToken);

            if (product is null)
            {
                throw new ProductNotFoundException();
            }

            product = product.Update(
                _clock.DateTimeOffset,
                command.Name,
                command.Description,
                command.Category,
                command.ImageFile,
                command.Price
            );

            _session.Update(product);
            await _session.SaveChangesAsync(cancellationToken);
            return new UpdateProductResult(true);
        }
        catch (Exception e)
        {
            _logger.LogError("Something went wrong on update product");
            return new UpdateProductResult(false);
        }
    }
}