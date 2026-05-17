namespace Ordering.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderHandler(IApplicationDbContext dbContext)
    : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = CreateNewOrder(command.Order);

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateOrderResult(order.Id.Value);
    }

    private static Order CreateNewOrder(OrderDto orderDto)
    {
        var newOrder = Order.Create(
            id: OrderId.Of(Guid.NewGuid()),
            customerId: CustomerId.Of(orderDto.CustomerId),
            orderName: OrderName.Of(orderDto.OrderName),
            shippingAddress: ToAddress(orderDto.ShippingAddress),
            billingAddress: ToAddress(orderDto.BillingAddress),
            payment: ToPayment(orderDto.Payment));

        foreach (var orderItemDto in orderDto.OrderItems)
        {
            newOrder.Add(ProductId.Of(orderItemDto.ProductId), orderItemDto.Quantity, orderItemDto.Price);
        }

        return newOrder;
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
