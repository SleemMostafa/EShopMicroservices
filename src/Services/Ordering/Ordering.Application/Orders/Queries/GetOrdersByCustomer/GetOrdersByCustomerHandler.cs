namespace Ordering.Application.Orders.Queries.GetOrdersByCustomer;
public sealed class GetOrdersByCustomerHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrdersByCustomerQuery, GetOrdersByCustomerResult>
{
    public async Task<GetOrdersByCustomerResult> Handle(GetOrdersByCustomerQuery query, CancellationToken cancellationToken)
    {
        var orders = await dbContext.Orders
            .Include(order => order.OrderItems)
            .AsNoTracking()
            .Where(order => order.CustomerId == CustomerId.Of(query.CustomerId))
            .OrderBy(order => order.OrderName.Value)
            .ToListAsync(cancellationToken);

        return new GetOrdersByCustomerResult(orders.ToOrderDtoList());
    }
}
