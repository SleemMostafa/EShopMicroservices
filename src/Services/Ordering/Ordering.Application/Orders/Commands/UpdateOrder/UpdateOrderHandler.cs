namespace Ordering.Application.Orders.Commands.UpdateOrder;

public sealed class UpdateOrderHandler(IApplicationDbContext dbContext)
    : ICommandHandler<UpdateOrderCommand, UpdateOrderResult>
{
    public async Task<UpdateOrderResult> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        var orderId = OrderId.Of(command.Order.Id);
        var order = await dbContext.Orders
            .FindAsync([orderId], cancellationToken: cancellationToken);

        if (order is null)
        {
            throw new OrderNotFoundException(command.Order.Id);
        }

        UpdateOrderWithNewValues(order, command.Order);

        dbContext.Orders.Update(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateOrderResult(true);
    }

    private static void UpdateOrderWithNewValues(Order order, OrderDto orderDto)
    {
        order.Update(
            orderName: OrderName.Of(orderDto.OrderName),
            shippingAddress: ToAddress(orderDto.ShippingAddress),
            billingAddress: ToAddress(orderDto.BillingAddress),
            payment: ToPayment(orderDto.Payment),
            status: orderDto.Status);
    }

    private static Address ToAddress(AddressDto address)
    {
        return Address.Of(
            address.FirstName,
            address.LastName,
            address.EmailAddress,
            address.AddressLine,
            address.Country,
            address.State,
            address.ZipCode);
    }

    private static Payment ToPayment(PaymentDto payment)
    {
        return Payment.Of(
            payment.CardName,
            payment.CardNumber,
            payment.Expiration,
            payment.Cvv,
            payment.PaymentMethod);
    }
}
