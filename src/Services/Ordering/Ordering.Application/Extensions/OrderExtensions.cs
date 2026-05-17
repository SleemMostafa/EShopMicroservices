namespace Ordering.Application.Extensions;

public static class OrderExtensions
{
    public static IEnumerable<OrderDto> ToOrderDtoList(this IEnumerable<Order> orders)
    {
        return orders.Select(ToOrderDto);
    }

    public static OrderDto ToOrderDto(this Order order)
    {
        return new OrderDto(
            Id: order.Id.Value,
            CustomerId: order.CustomerId.Value,
            OrderName: order.OrderName.Value,
            ShippingAddress: ToAddressDto(order.ShippingAddress),
            BillingAddress: ToAddressDto(order.BillingAddress),
            Payment: ToPaymentDto(order.Payment),
            Status: order.Status,
            OrderItems: order.OrderItems
                .Select(item => new OrderItemDto(item.OrderId.Value, item.ProductId.Value, item.Quantity, item.Price))
                .ToList());
    }

    private static AddressDto ToAddressDto(Address address)
    {
        return new AddressDto(
            address.FirstName,
            address.LastName,
            address.EmailAddress!,
            address.AddressLine,
            address.Country,
            address.State,
            address.ZipCode);
    }

    private static PaymentDto ToPaymentDto(Payment payment)
    {
        return new PaymentDto(
            payment.CardName!,
            payment.CardNumber,
            payment.Expiration,
            payment.CVV,
            payment.PaymentMethod);
    }
}
