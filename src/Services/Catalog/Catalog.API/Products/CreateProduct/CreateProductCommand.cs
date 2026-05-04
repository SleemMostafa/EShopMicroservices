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
internal sealed class CreateProductCommandHandler(
    IProductRepository repository,
    IEshopClock clock,
    ILogger<CreateProductCommandHandler> logger)
    : ICommandHandler<CreateProductCommand, CreateProductResult>
{
    public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("CreateProductCommandHandler.Handle called with {command}",command);
        var product = Product.Create(
            command.Name,
            command.Description,
            command.Category,
            command.ImageFile,
            command.Price,
            clock.DateTimeOffset);

        await repository.AddAsync(product, cancellationToken);

        logger.LogInformation("Create product successfully {id}", product.Id);
        return new CreateProductResult(product.Id);
    }
}
