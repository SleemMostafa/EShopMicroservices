using System.Linq.Expressions;

namespace BuildingBlocks.Paginated;

public sealed class PaginatedList<T>
{
    public PaginatedList(IReadOnlyList<T> items, PageInfo pageInfo)
    {
        Items = items;
        PageInfo = pageInfo;
    }

    public PageInfo PageInfo { get; }
    public IReadOnlyList<T> Items { get; }

    public static PaginatedList<T> Empty(int pageIndex = 0, int pageSize = 0)
    {
        return new PaginatedList<T>([], PageInfo.Empty(pageIndex, pageSize));
    }

    public static PaginatedList<T> GetEmpty()
    {
        return Empty();
    }

    public static Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source,
        PagingOptionsRequest request,
        bool all = false,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var totalCount = source.Count();
        if (totalCount == 0)
        {
            return Task.FromResult(Empty(request.PageIndexOrDefault, request.PageSize));
        }

        var items = request.Apply(source, all).ToList();

        return Task.FromResult(new PaginatedList<T>(items, request.GetPageInfo(totalCount)));
    }

    public static Task<PaginatedList<TDto>> CreateAsync<TDto>(
        IQueryable<T> source,
        Expression<Func<T, TDto>> map,
        PagingOptionsRequest request,
        bool all = false,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var totalCount = source.Count();
        if (totalCount == 0)
        {
            return Task.FromResult(PaginatedList<TDto>.Empty(request.PageIndexOrDefault, request.PageSize));
        }

        var items = request.Apply(source, all).Select(map).ToList();

        return Task.FromResult(new PaginatedList<TDto>(items, request.GetPageInfo(totalCount)));
    }

    public static Task<(IQueryable<T> Queryable, PageInfo Info)> CreateQueryableAsync(
        IQueryable<T> source,
        PagingOptionsRequest request,
        bool all = false,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var totalCount = source.Count();
        var queryable = totalCount == 0
            ? Enumerable.Empty<T>().AsQueryable()
            : request.Apply(source, all);

        return Task.FromResult((queryable, request.GetPageInfo(totalCount)));
    }

}
