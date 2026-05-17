using BuildingBlocks.Paginated;

namespace Ordering.Application.Orders.Queries.GetOrders;

public record GetOrdersQuery(PagingOptionsRequest PagingOptions)
    : IQuery<GetOrdersResult>;

public record GetOrdersResult(PaginatedList<OrderDto> Orders);
