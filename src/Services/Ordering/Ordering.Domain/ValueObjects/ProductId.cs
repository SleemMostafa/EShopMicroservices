namespace Ordering.Domain.ValueObjects;

public sealed class ProductId : ValueObject
{
    public Guid Value { get; }

    private ProductId(Guid value)
    {
        Value = value;
    }

    public static ProductId Of(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("ProductId cannot be empty.");
        }

        return new ProductId(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
