namespace BuildingBlocks;

public interface IEshopClock
{
    DateTimeOffset DateTimeOffset { get; }

    DateTime DateTime { get; }
}
public sealed class EshopClock:IEshopClock
{

    public DateTime DateTime => DateTime.UtcNow;

    public DateTimeOffset DateTimeOffset => DateTimeOffset.UtcNow;
}