using ArgumentValidator;
using BuildingBlocks.DDD;

namespace Basket.API.Domain.ShoppingCarts;

public sealed class ShoppingCartItem : ValueObject
{
    private ShoppingCartItem()
    {
    }

    private ShoppingCartItem(
        int quantity,
        string color,
        decimal price,
        Guid productId,
        string productName)
    {
        Quantity = quantity;
        Color = color;
        Price = price;
        ProductId = productId;
        ProductName = productName;
    }

    public int Quantity { get; private set; }
    public string Color { get; private set; } = default!;
    public decimal Price { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;

    public static ShoppingCartItem Create(
        int quantity,
        string color,
        decimal price,
        Guid productId,
        string productName)
    {
        try
        {
            Throw.IfNot(() => quantity > 0);
            Throw.IfNullOrEmpty(color, nameof(color));
            Throw.IfNot(() => price >= 0);
            Throw.IfNot(() => productId != Guid.Empty);
            Throw.IfNullOrEmpty(productName, nameof(productName));

            return new ShoppingCartItem(
                quantity,
                color.Trim(),
                price,
                productId,
                productName.Trim());
        }
        catch (ArgumentException exception)
        {
            throw new BadRequestException("Invalid basket item data.", exception.Message);
        }
    }

    public void UpdatePrice(decimal price)
    {
        try
        {
            Throw.IfNot(() => price >= 0);
            Price = price;
        }
        catch (ArgumentException exception)
        {
            throw new BadRequestException("Invalid basket item price.", exception.Message);
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Quantity;
        yield return Color;
        yield return Price;
        yield return ProductId;
        yield return ProductName;
    }
}
