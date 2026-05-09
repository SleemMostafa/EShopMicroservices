using System.Linq.Expressions;
using FluentValidation;

namespace BuildingBlocks.Paginated;

public class PagingOptionsRequest
{
    public const int DefaultIndex = 0;
    public const int DefaultPageSize = 25;
    public const int MaxPageSize = 100;

    public int? Offset { get; set; }
    public int? PageIndex { get; set; }
    public string? SortBy { get; set; }
    public bool Ascending { get; set; } = true;

    public int PageIndexOrDefault => Math.Max(PageIndex ?? DefaultIndex, 0);
    public int PageSize => NormalizePageSize(Offset);

    public virtual string CacheParams =>
        $"i{PageIndexOrDefault}x{PageSize},sort{NormalizeSortBy(SortBy) ?? "-"}/{(Ascending ? 1 : 0)}";

    public PageInfo GetPageInfo(int totalCount)
    {
        return PageInfo.Create(PageIndexOrDefault, PageSize, totalCount);
    }

    public IQueryable<T> Apply<T>(IQueryable<T> query, bool all = false)
    {
        var sortedQuery = ApplySorting(query);

        return all ? sortedQuery : ApplyPaging(sortedQuery);
    }

    public IQueryable<T> Handle<T>(IQueryable<T> query, bool all)
    {
        return Apply(query, all);
    }

    private IQueryable<T> ApplyPaging<T>(IQueryable<T> query)
    {
        return query
            .Skip(PageIndexOrDefault * PageSize)
            .Take(PageSize);
    }

    private IQueryable<T> ApplySorting<T>(IQueryable<T> query)
    {
        var sortBy = NormalizeSortBy(SortBy);
        if (sortBy is null)
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "item");
        var propertyAccess = BuildPropertyAccess(parameter, sortBy);

        if (propertyAccess is null)
        {
            return query;
        }

        var lambda = Expression.Lambda(propertyAccess, parameter);
        var methodName = Ascending ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending);
        var method = typeof(Queryable)
            .GetMethods()
            .Single(method =>
                method.Name == methodName &&
                method.GetParameters().Length == 2);

        var genericMethod = method.MakeGenericMethod(typeof(T), propertyAccess.Type);

        return (IQueryable<T>)genericMethod.Invoke(null, [query, lambda])!;
    }

    private static Expression? BuildPropertyAccess(Expression parameter, string propertyPath)
    {
        Expression current = parameter;

        foreach (var memberName in propertyPath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var property = current.Type.GetProperties()
                .FirstOrDefault(property => string.Equals(property.Name, memberName, StringComparison.OrdinalIgnoreCase));

            if (property is null)
            {
                return null;
            }

            current = Expression.Property(current, property);
        }

        return current;
    }

    private static int NormalizePageSize(int? pageSize)
    {
        if (pageSize is null or <= 0)
        {
            return DefaultPageSize;
        }

        return Math.Min(pageSize.Value, MaxPageSize);
    }

    private static string? NormalizeSortBy(string? sortBy)
    {
        return string.IsNullOrWhiteSpace(sortBy) ? null : sortBy.Trim();
    }

}

public sealed class PagingOptionsRequestValidator : AbstractValidator<PagingOptionsRequest>
{
    public PagingOptionsRequestValidator()
    {
        RuleFor(request => request.Offset)
            .InclusiveBetween(1, PagingOptionsRequest.MaxPageSize)
            .When(request => request.Offset.HasValue);

        RuleFor(request => request.PageIndex)
            .GreaterThanOrEqualTo(0)
            .When(request => request.PageIndex.HasValue);

        RuleFor(request => request.SortBy)
            .MaximumLength(128)
            .Matches("^[a-zA-Z0-9_.]+$")
            .When(request => !string.IsNullOrWhiteSpace(request.SortBy));
    }
}

public sealed class SearchPagingOptionsRequest : PagingOptionsRequest
{
    public string? Search { get; init; }

    public override string CacheParams => $"{base.CacheParams},search{Search?.Trim() ?? "-"}";

    public IQueryable<T> ApplyQuery<T>(
        IQueryable<T> query,
        params Expression<Func<T, string?>>[] searchExpressions)
    {
        if (string.IsNullOrWhiteSpace(Search) || searchExpressions.Length == 0)
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "item");
        var searchValue = Search.Trim().ToLowerInvariant();
        var containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
        var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;

        Expression? combinedExpression = null;

        foreach (var searchExpression in searchExpressions)
        {
            var body = ReplaceParameter(searchExpression.Body, searchExpression.Parameters[0], parameter);
            var guardedBody = Expression.Coalesce(body, Expression.Constant(string.Empty));
            var comparison = Expression.Call(
                Expression.Call(guardedBody, toLowerMethod),
                containsMethod,
                Expression.Constant(searchValue));

            combinedExpression = combinedExpression is null
                ? comparison
                : Expression.OrElse(combinedExpression, comparison);
        }

        if (combinedExpression is null)
        {
            return query;
        }

        return query.Where(Expression.Lambda<Func<T, bool>>(combinedExpression, parameter));
    }

    private static Expression ReplaceParameter(
        Expression expression,
        ParameterExpression oldParameter,
        ParameterExpression newParameter)
    {
        return new ParameterReplaceVisitor(oldParameter, newParameter).Visit(expression)!;
    }

    private sealed class ParameterReplaceVisitor(
        ParameterExpression oldParameter,
        ParameterExpression newParameter) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == oldParameter ? newParameter : base.VisitParameter(node);
        }
    }
}
