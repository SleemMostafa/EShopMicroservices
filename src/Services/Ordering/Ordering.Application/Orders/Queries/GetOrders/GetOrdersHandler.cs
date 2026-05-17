using BuildingBlocks.Paginated;

namespace Ordering.Application.Orders.Queries.GetOrders;

public sealed class GetOrdersHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrdersQuery, GetOrdersResult>
{
    public async Task<GetOrdersResult> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.Orders
            .Include(order => order.OrderItems)
            .AsNoTracking()
            .OrderBy(order => order.OrderName.Value);

        var totalCount = await source.CountAsync(cancellationToken);
        var orders = await query.PagingOptions
            .Apply(source)
            .ToListAsync(cancellationToken);

        return new GetOrdersResult(
            new PaginatedList<OrderDto>(
                orders.ToOrderDtoList().ToList(),
                query.PagingOptions.GetPageInfo(totalCount)));
    }
}
