namespace Basket.API.Exceptions;

public sealed class BasketNotFoundException(string message) : NotFoundException(message)
{
   
}
