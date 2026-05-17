namespace Ordering.Domain.ValueObjects;

public sealed class OrderItemId : ValueObject
{
    public Guid Value { get; }

    private OrderItemId(Guid value)
    {
        Value = value;
    }

    public static OrderItemId Of(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("OrderItemId cannot be empty.");
        }

        return new OrderItemId(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
