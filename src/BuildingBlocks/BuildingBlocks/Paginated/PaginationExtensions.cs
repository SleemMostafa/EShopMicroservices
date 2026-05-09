using System.Linq.Expressions;

namespace BuildingBlocks.Paginated;

public static class PaginationExtensions
{
    public static Task<PaginatedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> source,
        PagingOptionsRequest request,
        bool all = false,
        CancellationToken cancellationToken = default)
    {
        return PaginatedList<T>.CreateAsync(source, request, all, cancellationToken);
    }

    public static Task<PaginatedList<TDto>> ToPagedListAsync<T, TDto>(
        this IQueryable<T> source,
        Expression<Func<T, TDto>> map,
        PagingOptionsRequest request,
        bool all = false,
        CancellationToken cancellationToken = default)
    {
        return PaginatedList<T>.CreateAsync(source, map, request, all, cancellationToken);
    }

    public static Task<(IQueryable<T> Queryable, PageInfo Info)> ToPagedQueryableAsync<T>(
        this IQueryable<T> source,
        PagingOptionsRequest request,
        bool all = false,
        CancellationToken cancellationToken = default)
    {
        return PaginatedList<T>.CreateQueryableAsync(source, request, all, cancellationToken);
    }
}
