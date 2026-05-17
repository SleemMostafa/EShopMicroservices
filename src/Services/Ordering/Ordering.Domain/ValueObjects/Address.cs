namespace Ordering.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public string FirstName { get; } = default!;
    public string LastName { get; } = default!;
    public string? EmailAddress { get; } = default!;
    public string AddressLine { get; } = default!;
    public string Country { get; } = default!;
    public string State { get; } = default!;
    public string ZipCode { get; } = default!;

    private Address()
    {
    }

    private Address(
        string firstName,
        string lastName,
        string emailAddress,
        string addressLine,
        string country,
        string state,
        string zipCode)
    {
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
        AddressLine = addressLine;
        Country = country;
        State = state;
        ZipCode = zipCode;
    }

    public static Address Of(
        string firstName,
        string lastName,
        string emailAddress,
        string addressLine,
        string country,
        string state,
        string zipCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emailAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(addressLine);

        return new Address(firstName, lastName, emailAddress, addressLine, country, state, zipCode);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
        yield return EmailAddress;
        yield return AddressLine;
        yield return Country;
        yield return State;
        yield return ZipCode;
    }
}
