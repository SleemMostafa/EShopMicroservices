using MediatR;

namespace BuildingBlocks.DDD;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
