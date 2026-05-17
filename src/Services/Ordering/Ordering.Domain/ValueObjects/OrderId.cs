namespace Ordering.Domain.ValueObjects;

public sealed class OrderId : ValueObject
{
    public Guid Value { get; }

    private OrderId(Guid value)
    {
        Value = value;
    }

    public static OrderId Of(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("OrderId cannot be empty.");
        }

        return new OrderId(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
