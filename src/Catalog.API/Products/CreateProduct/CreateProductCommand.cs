namespace Catalog.API.Products.CreateProduct;

public sealed record CreateProductCommand
    (
        string Name,
        string Description,
        List<string> Category,
        string ImageFile,
        decimal Price) : ICommand<CreateProductResult>;

public sealed record CreateProductResult(Guid Id);

internal sealed class CreateProductCommandHandler:ICommandHandler<CreateProductCommand,CreateProductResult>
{
    private readonly IDocumentSession _session;
    private readonly IEshopClock _clock;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(IDocumentSession session, IEshopClock clock, ILogger<CreateProductCommandHandler> logger)
    {
        _session = session;
        _clock = clock;
        _logger = logger;
    }

    public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = command.Name,
                Category = command.Category,
                Description = command.Description,
                Price = command.Price,
                ImageFile = command.ImageFile,
                DateCreated = _clock.DateTimeOffset,
            };
        
             _session.Store(product);
            await _session.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Create product successfully {id}",product.Id);
            return new CreateProductResult(product.Id);
        }
        catch (Exception e)
        {
            _logger.LogError("Something went wrong on create product {message}",e.Message);
            return new CreateProductResult(Guid.Empty);
        }
    }
}