namespace Basket.API.Basket.DeleteBasket;

public sealed record DeleteBasketCommand(string UserName) : ICommand<DeleteBasketResult>;
public sealed record DeleteBasketResult(bool IsSuccess);

public sealed class DeleteBasketCommandValidator : AbstractValidator<DeleteBasketCommand>
{
    public DeleteBasketCommandValidator()
    {
        RuleFor(command => command.UserName).NotEmpty().WithMessage("UserName is required");
    }
}

public sealed class DeleteBasketCommandHandler(IDocumentSession session, ICacheService cache, ILogger<DeleteBasketCommandHandler> logger)
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
