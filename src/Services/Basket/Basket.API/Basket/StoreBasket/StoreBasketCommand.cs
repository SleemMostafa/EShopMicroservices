using Discount.Grpc;

namespace Basket.API.Basket.StoreBasket;

public sealed record StoreBasketCommand(ShoppingCart Cart) : ICommand<StoreBasketResult>;
public sealed record StoreBasketResult(string UserName);

public sealed class StoreBasketCommandValidator : AbstractValidator<StoreBasketCommand>
{
    public StoreBasketCommandValidator()
    {
        RuleFor(command => command.Cart).NotNull();
        RuleFor(command => command.Cart.UserName).NotEmpty();
        RuleFor(command => command.Cart.Items).NotEmpty();
    }
}

public sealed class StoreBasketCommandHandler(
    IDocumentSession session,
    ICacheService cache,
    DiscountProtoService.DiscountProtoServiceClient discountProto,
    ILogger<StoreBasketCommandHandler> logger)
    : ICommandHandler<StoreBasketCommand, StoreBasketResult>
{
    public async Task<StoreBasketResult> Handle(StoreBasketCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Storing basket for {UserName}", command.Cart.UserName);

        await DeductDiscount(command.Cart, cancellationToken);

        session.Store(command.Cart);
        await session.SaveChangesAsync(cancellationToken);

        await cache.SetAsync(command.Cart.UserName, BasketMapper.ToDto(command.Cart), cancellationToken);

        return new StoreBasketResult(command.Cart.UserName);
    }

    private async Task DeductDiscount(ShoppingCart cart, CancellationToken cancellationToken)
    {
        foreach (var item in cart.Items)
        {
            var coupon = await discountProto.GetDiscountAsync(
                new GetDiscountRequest { ProductName = item.ProductName },
                cancellationToken: cancellationToken);
            var newPrice = item.Price - coupon.Amount;
            item.UpdatePrice(newPrice);
        }
    }
}
