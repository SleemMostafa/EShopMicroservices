namespace BuildingBlocks.Behaviors;

public sealed class IdempotencyOptions
{
    public const string SectionName = "Idempotency";

    public bool Enabled { get; init; } = true;

    public int ExpirationMinutes { get; init; } = 60;
}
