namespace Ordering.Domain.ValueObjects;

public sealed class OrderName : ValueObject
{
    public string Value { get; } = default!;

    private OrderName(string value)
    {
        Value = value;
    }

    public static OrderName Of(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new OrderName(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
