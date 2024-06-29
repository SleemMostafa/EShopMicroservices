namespace Basket.API.Models
{
    public class ShoppingCartItem
    {
        public required int Quantity { get; init; } 
        public required string Color { get; init; } 
        public required decimal Price { get; init; } 
        public required Guid ProductId { get; init; } 
        public required string ProductName { get; init; } 
    }
}
