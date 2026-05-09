namespace Basket.API.Dtos;

public sealed record ShoppingCartDto(
    string UserName,
    IReadOnlyList<ShoppingCartItemDto> Items,
    decimal TotalPrice);

public sealed record ShoppingCartItemDto(
    int Quantity,
    string Color,
    decimal Price,
    Guid ProductId,
    string ProductName);
