namespace BuildingBlocks.Authentication;

public sealed class InternalServiceAuthOptions
{
    public const string SectionName = "InternalServiceAuth";

    public string ServiceName { get; init; } = default!;

    public string[] Scopes { get; init; } = [];

    public int TokenLifetimeMinutes { get; init; } = 5;
}
