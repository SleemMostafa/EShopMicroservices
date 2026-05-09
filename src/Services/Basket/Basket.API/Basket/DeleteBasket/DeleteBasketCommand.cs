using Microsoft.Extensions.Caching.Distributed;

namespace Basket.API.Basket.DeleteBasket;

public record DeleteBasketCommand(string UserName) : ICommand<DeleteBasketResult>;
public record DeleteBasketResult(bool IsSuccess);

public sealed class DeleteBasketCommandValidator : AbstractValidator<DeleteBasketCommand>
{
    public DeleteBasketCommandValidator()
    {
        RuleFor(command => command.UserName).NotEmpty().WithMessage("UserName is required");
    }
}

public sealed class DeleteBasketCommandHandler(IDocumentSession session, IDistributedCache cache, ILogger<DeleteBasketCommandHandler> logger)
    : ICommandHandler<DeleteBasketCommand, DeleteBasketResult>
{
    public async Task<DeleteBasketResult> Handle(DeleteBasketCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting basket for {UserName}", command.UserName);

        session.Delete<ShoppingCart>(command.UserName);
        await session.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(command.UserName, cancellationToken);

        return new DeleteBasketResult(true);
    }
}
