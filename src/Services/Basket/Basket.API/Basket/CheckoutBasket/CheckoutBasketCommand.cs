namespace Basket.API.Basket.CheckoutBasket;

public sealed record CheckoutBasketCommand(BasketCheckoutDto BasketCheckoutDto)
    : ICommand<CheckoutBasketResult>;
public record CheckoutBasketResult(bool IsSuccess);

public sealed class CheckoutBasketCommandValidator
    : AbstractValidator<CheckoutBasketCommand>
{
    public CheckoutBasketCommandValidator()
    {
        RuleFor(command => command.BasketCheckoutDto).NotNull();
        RuleFor(command => command.BasketCheckoutDto.UserName).NotEmpty();
    }
}

public sealed class CheckoutBasketCommandHandler(IDocumentSession session, ICacheService cache, ILogger<CheckoutBasketCommandHandler> logger)
    : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
{
    public async Task<CheckoutBasketResult> Handle(CheckoutBasketCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking out basket for {UserName}", command.BasketCheckoutDto.UserName);

        var basket = await session.LoadAsync<ShoppingCart>(command.BasketCheckoutDto.UserName, cancellationToken);
        if (basket is null)
        {
            return new CheckoutBasketResult(false);
        }

        session.Delete<ShoppingCart>(command.BasketCheckoutDto.UserName);
        await session.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(command.BasketCheckoutDto.UserName, cancellationToken);

        return new CheckoutBasketResult(true);
    }
}
