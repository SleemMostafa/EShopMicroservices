using System.Linq.Expressions;

namespace BuildingBlocks.Extensions;

public static class ReflectionExtensions
{
    /// <summary>
    /// Creates a selector expression using a property name e.g. c => c.Property
    /// </summary>
    /// <param name="type">Just a placeholder that means nothing</param>
    /// <param name="propertyName">It's the navigation path (e.g. Value.Length) to produce c => c.Value.Length</param>
    public static LambdaExpression? CreateSelectorExpression<T>(this Type type, string? propertyName)
    {
        var parameterExpression = Expression.Parameter(typeof(T));

        var members = propertyName?.Split('-').ToList() ?? new List<string>();

        if (members.Count == 0)
            return null;

        try
        {
            var member = members[0];

            var innerMembers = member?.Split('.').ToList() ?? new List<string>();

            var memberExpression = Expression.PropertyOrField(parameterExpression, innerMembers[0]);

            if (innerMembers.Count > 1)
                memberExpression = Expression.PropertyOrField(memberExpression, innerMembers[1]);

            if (members.Count > 1)
            {
                memberExpression = members.Skip(1).ToList().Aggregate(memberExpression, Expression.PropertyOrField);
            }

            return Expression.Lambda(memberExpression, parameterExpression);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Aggregates the passed in array of expressions using the Or expression
    /// </summary>
    public static BinaryExpression Or(Expression[] comparisonExpressions)
    {
        if (comparisonExpressions.Length == 0)
            throw new ArgumentException("Binary expressions require at least two operands");

        if (comparisonExpressions.Length < 2)
            return Expression.OrElse(comparisonExpressions[0], Expression.Constant(false));

        var or = Expression.OrElse(comparisonExpressions[0], comparisonExpressions[1]);

        for (var i = 2; i < comparisonExpressions.Length; i++)
            or = Expression.OrElse(or, comparisonExpressions[i]);

        return or;
    }
}
