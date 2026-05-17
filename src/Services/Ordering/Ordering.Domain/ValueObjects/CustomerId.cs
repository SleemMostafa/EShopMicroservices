namespace Ordering.Domain.ValueObjects;

public sealed class CustomerId : ValueObject
{
    public Guid Value { get; }

    private CustomerId(Guid value)
    {
        Value = value;
    }

    public static CustomerId Of(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("CustomerId cannot be empty.");
        }

        return new CustomerId(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
