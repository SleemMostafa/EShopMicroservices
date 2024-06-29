using FluentValidation;

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

public sealed class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
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