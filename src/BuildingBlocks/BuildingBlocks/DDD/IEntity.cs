namespace BuildingBlocks.DDD;

public interface IEntity<out TId>
{
    TId Id { get; }
}
