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
            .NotEmpty()
            .Length(2, 150);
        
        RuleFor(command => command.Description)
            .NotEmpty()
            .Length(10, 500);

        RuleFor(command => command.ImageFile)
            .NotEmpty();
        
        RuleFor(command => command.Category)
            .NotEmpty();
        
        RuleFor(command => command.Price)
            .GreaterThan(0);
    }
}
internal sealed class UpdateProductCommandHandler(
    IDocumentSession session,
    ICurrentUserProvider currentUserProvider,
    ILogger<UpdateProductCommandHandler> logger,
    IEshopClock clock)
    : ICommandHandler<UpdateProductCommand, UpdateProductResult>
{
    public async Task<UpdateProductResult> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("UpdateProductCommandHandler.Handle called with {command}", command);

        var product = await session.LoadAsync<Product>(command.Id, cancellationToken);

        if (product is null)
        {
            throw new ProductNotFoundException(command.Id);
        }

        product.Update(
            clock.DateTimeOffset,
            command.Name,
            command.Description,
            command.Category,
            command.ImageFile,
            command.Price);

        session.Update(product);
        session.Store(ProductAuditTrail.Create(
            product.Id,
            ProductAuditAction.Updated,
            currentUserProvider.UserId,
            clock.DateTimeOffset));

        await session.SaveChangesAsync(cancellationToken);
        return new UpdateProductResult(true);
    }
}
