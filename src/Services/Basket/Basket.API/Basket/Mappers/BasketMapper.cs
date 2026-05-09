using Basket.API.Basket.CheckoutBasket;
using Basket.API.Basket.DeleteBasket;
using Basket.API.Basket.GetBasket;
using Basket.API.Basket.StoreBasket;

namespace Basket.API.Basket.Mappers;

public static class BasketMapper
{
    public static GetBasketResponse ToResponse(GetBasketResult result)
    {
        return new GetBasketResponse(ToDto(result.Cart));
    }

    public static StoreBasketCommand ToCommand(StoreBasketRequest request)
    {
        return new StoreBasketCommand(ToDomain(request.Cart));
    }

    public static StoreBasketResponse ToResponse(StoreBasketResult result)
    {
        return new StoreBasketResponse(result.UserName);
    }

    public static DeleteBasketResponse ToResponse(DeleteBasketResult result)
    {
        return new DeleteBasketResponse(result.IsSuccess);
    }

    public static CheckoutBasketCommand ToCommand(CheckoutBasketRequest request)
    {
        return new CheckoutBasketCommand(request.BasketCheckoutDto);
    }

    public static CheckoutBasketResponse ToResponse(CheckoutBasketResult result)
    {
        return new CheckoutBasketResponse(result.IsSuccess);
    }

    public static ShoppingCartDto ToDto(ShoppingCart cart)
    {
        return new ShoppingCartDto(
            cart.UserName,
            cart.Items.Select(ToDto).ToList(),
            cart.TotalPrice);
    }

    public static ShoppingCartItemDto ToDto(ShoppingCartItem item)
    {
        return new ShoppingCartItemDto(
            item.Quantity,
            item.Color,
            item.Price,
            item.ProductId,
            item.ProductName);
    }

    public static ShoppingCart ToDomain(ShoppingCartDto cart)
    {
        return ShoppingCart.Create(
            cart.UserName,
            cart.Items.Select(ToDomain));
    }

    public static ShoppingCartItem ToDomain(ShoppingCartItemDto item)
    {
        return ShoppingCartItem.Create(
            item.Quantity,
            item.Color,
            item.Price,
            item.ProductId,
            item.ProductName);
    }
}
