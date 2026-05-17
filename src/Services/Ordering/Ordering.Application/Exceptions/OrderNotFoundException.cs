using BuildingBlocks.Exceptions;

namespace Ordering.Application.Exceptions;
public sealed class OrderNotFoundException : NotFoundException
{
    public OrderNotFoundException(Guid id) : base("Order", id)
    {
    }
}
