namespace Basket.API.Models
{
    public class ShoppingCart
    {
        public required string UserName { get; init; }
        public List<ShoppingCartItem> Items { get; init; } = [];
        public decimal TotalPrice => Items.Sum(x => x.Price * x.Quantity);

        public ShoppingCart(string userName)
        {
            UserName = userName;
        }

        //Required for Mapping
        public ShoppingCart()
        {
        }
    }
}
