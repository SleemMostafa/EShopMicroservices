namespace Ordering.Domain.Events;

public sealed record OrderCreatedEvent(Order Order) : DomainEvent;
