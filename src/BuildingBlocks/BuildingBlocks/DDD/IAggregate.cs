namespace BuildingBlocks.DDD;

public interface IAggregate<out TId> : IEntity<TId>
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    IDomainEvent[] ClearDomainEvents();
}
