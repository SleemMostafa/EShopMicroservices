using BuildingBlocks.CQRS;

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
    public Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}