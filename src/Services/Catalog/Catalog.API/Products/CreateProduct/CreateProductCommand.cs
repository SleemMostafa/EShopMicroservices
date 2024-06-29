using FluentValidation;

namespace Catalog.API.Products.CreateProduct;

public sealed record CreateProductCommand
    (
        string Name,
        string Description,
        List<string> Category,
        string ImageFile,
        decimal Price) : ICommand<CreateProductResult>;

public sealed record CreateProductResult(Guid Id);

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(2, 150).WithMessage("Name must be between 2 and 150 characters");
        
        RuleFor(command => command.Description)
            .NotEmpty().WithMessage("Description is required")
            .Length(10, 500).WithMessage("Description must be between 10 and 500 characters");

        RuleFor(command => command.ImageFile)
            .NotEmpty().WithMessage("ImageFile is required");
        
        RuleFor(command => command.Category)
            .NotEmpty().WithMessage("Category is required");
        
        RuleFor(command => command.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");
    }
}
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
        _logger.LogInformation("CreateProductCommandHandler.Handle called with {command}",command);
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