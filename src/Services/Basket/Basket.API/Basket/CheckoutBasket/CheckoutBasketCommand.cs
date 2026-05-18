namespace Basket.API.Basket.CheckoutBasket;

public sealed record CheckoutBasketCommand(BasketCheckoutDto BasketCheckoutDto, string? IdempotencyKey = null)
    : IIdempotentCommand<CheckoutBasketResult>;
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

public sealed class CheckoutBasketCommandHandler(
    IDocumentSession session,
    ICacheService cache,
    IPublishEndpoint publishEndpoint,
    ILogger<CheckoutBasketCommandHandler> logger)
    : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
{
    public async Task<CheckoutBasketResult> Handle(CheckoutBasketCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking out basket for {UserName}", command.BasketCheckoutDto.UserName);

        var basket = await session.LoadAsync<ShoppingCart>(command.BasketCheckoutDto.UserName, cancellationToken);
        if (basket is null || basket.Items.Count == 0)
        {
            return new CheckoutBasketResult(false);
        }

        var checkoutEvent = ToBasketCheckoutEvent(command.BasketCheckoutDto, basket);

        session.Delete<ShoppingCart>(command.BasketCheckoutDto.UserName);
        await session.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(command.BasketCheckoutDto.UserName, cancellationToken);
        await publishEndpoint.Publish(checkoutEvent, publishContext =>
        {
            if (Guid.TryParse(command.IdempotencyKey, out var correlationId))
            {
                publishContext.CorrelationId = correlationId;
            }
        }, cancellationToken);

        return new CheckoutBasketResult(true);
    }

    private static BasketCheckoutEvent ToBasketCheckoutEvent(
        BasketCheckoutDto checkout,
        ShoppingCart basket)
    {
        return new BasketCheckoutEvent
        {
            UserName = checkout.UserName,
            CustomerId = checkout.CustomerId,
            TotalPrice = basket.TotalPrice,
            FirstName = checkout.FirstName,
            LastName = checkout.LastName,
            EmailAddress = checkout.EmailAddress,
            AddressLine = checkout.AddressLine,
            Country = checkout.Country,
            State = checkout.State,
            ZipCode = checkout.ZipCode,
            CardName = checkout.CardName,
            CardNumber = checkout.CardNumber,
            Expiration = checkout.Expiration,
            CVV = checkout.CVV,
            PaymentMethod = checkout.PaymentMethod,
            Items = basket.Items
                .Select(item => new BasketCheckoutItemEvent(
                    item.ProductId,
                    item.Quantity,
                    item.Price,
                    item.ProductName))
                .ToList()
        };
    }
}
