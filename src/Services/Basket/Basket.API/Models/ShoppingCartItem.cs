namespace Basket.API.Models
{
    public class ShoppingCartItem
    {
        public required int Quantity { get; init; } 
        public required string Color { get; init; } 
        public decimal Price { get;  private set; } 
        public required Guid ProductId { get; init; } 
        public required string ProductName { get; init; }

        public void UpdatePrice(decimal price)
        {
            Price = price;
        }
    }
}
