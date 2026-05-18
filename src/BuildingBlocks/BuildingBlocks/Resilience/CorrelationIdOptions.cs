namespace BuildingBlocks.Resilience;

public sealed class CorrelationIdOptions
{
    public const string SectionName = "Correlation";

    public string HeaderName { get; init; } = "X-Correlation-ID";
}
