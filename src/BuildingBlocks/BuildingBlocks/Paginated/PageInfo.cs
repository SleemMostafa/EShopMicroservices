namespace BuildingBlocks.Paginated;

public sealed class PageInfo
{
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public int TotalCount { get; init; }
    public bool HasPreviousPage => PageIndex > 0;
    public bool HasNextPage => PageIndex + 1 < TotalPages;

    public static PageInfo Create(int pageIndex, int pageSize, int totalCount)
    {
        return new PageInfo
        {
            PageIndex = Math.Max(pageIndex, 0),
            PageSize = Math.Max(pageSize, 0),
            TotalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize),
            TotalCount = Math.Max(totalCount, 0)
        };
    }

    public static PageInfo Empty(int pageIndex = 0, int pageSize = 0)
    {
        return Create(pageIndex, pageSize, 0);
    }
}
