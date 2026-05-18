using FluentValidation;

namespace Catalog.API.Products.CreateProduct;

public sealed record CreateProductCommand
    (
        string Name,
        string Description,
        List<string> Category,
        string ImageFile,
        decimal Price,
        string? IdempotencyKey = null) : IIdempotentCommand<CreateProductResult>;

public sealed record CreateProductResult(Guid Id);

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
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
internal sealed class CreateProductCommandHandler(
    IDocumentSession session,
    ICurrentUserProvider currentUserProvider,
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

        session.Store(product);
        session.Store(ProductAuditTrail.Create(
            product.Id,
            ProductAuditAction.Created,
            currentUserProvider.UserId,
            clock.DateTimeOffset));

        await session.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Create product successfully {id}", product.Id);
        return new CreateProductResult(product.Id);
    }
}
