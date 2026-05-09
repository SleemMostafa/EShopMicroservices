using ArgumentValidator;
using BuildingBlocks.DDD;

namespace Basket.API.Domain.ShoppingCarts;

public sealed class ShoppingCart : Aggregate<string>
{
    private ShoppingCart()
    {
    }

    private ShoppingCart(string userName, List<ShoppingCartItem> items)
    {
        Id = userName;
        Items = items;
    }

    public string UserName => Id;
    public List<ShoppingCartItem> Items { get; private set; } = [];
    public decimal TotalPrice => Items.Sum(item => item.Price * item.Quantity);

    public static ShoppingCart Create(string userName, IEnumerable<ShoppingCartItem> items)
    {
        try
        {
            Throw.IfNullOrEmpty(userName, nameof(userName));
            Throw.IfNull(items, nameof(items));

            var normalizedItems = items.ToList();
            Throw.IfNot(() => normalizedItems.Count != 0);

            return new ShoppingCart(userName.Trim(), normalizedItems);
        }
        catch (ArgumentException exception)
        {
            throw new BadRequestException("Invalid basket data.", exception.Message);
        }
    }
}
