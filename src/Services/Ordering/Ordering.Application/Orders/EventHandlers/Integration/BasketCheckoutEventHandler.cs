using BuildingBlocks.Messaging.Events;
using MassTransit;
using Ordering.Application.Orders.Commands.CreateOrder;

namespace Ordering.Application.Orders.EventHandlers.Integration;
public sealed class BasketCheckoutEventHandler
    (ISender sender, ILogger<BasketCheckoutEventHandler> logger)
    : IConsumer<BasketCheckoutEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        logger.LogInformation(
            "Creating order from integration event {IntegrationEvent} with MessageId {MessageId}",
            context.Message.GetType().Name,
            context.MessageId);

        var command = MapToCreateOrderCommand(context.Message) with
        {
            IdempotencyKey = context.MessageId?.ToString() ??
                             context.CorrelationId?.ToString()
        };
        var result = await sender.Send(command, context.CancellationToken);

        logger.LogInformation(
            "Order {OrderId} created from basket checkout event {IntegrationEventId}",
            result.Id,
            context.Message.Id);
    }

    private CreateOrderCommand MapToCreateOrderCommand(BasketCheckoutEvent message)
    {
        // Create full order with incoming event data
        var addressDto = new AddressDto(message.FirstName, message.LastName, message.EmailAddress, message.AddressLine, message.Country, message.State, message.ZipCode);
        var paymentDto = new PaymentDto(message.CardName, message.CardNumber, message.Expiration, message.CVV, message.PaymentMethod);
        var orderId = Guid.NewGuid();

        var orderDto = new OrderDto(
            Id: orderId,
            CustomerId: message.CustomerId,
            OrderName: message.UserName,
            ShippingAddress: addressDto,
            BillingAddress: addressDto,
            Payment: paymentDto,
            Status: Ordering.Domain.Enums.OrderStatus.Pending,
            OrderItems: message.Items
                .Select(item => new OrderItemDto(orderId, item.ProductId, item.Quantity, item.Price))
                .ToList());

        return new CreateOrderCommand(orderDto);
    }
}
