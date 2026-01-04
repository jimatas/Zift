namespace Zift;

using Pagination.Cursor;
using Pagination.Offset;
using Querying.ExpressionBuilding;
using Querying.Parsing;

public static class QueryableExtensions
{
    public static IQueryable<T> Where<T>(
        this IQueryable<T> source,
        string whereExpression,
        ExpressionBuilderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (string.IsNullOrWhiteSpace(whereExpression))
        {
            return source;
        }

        var predicateNode =
            new ExpressionParser(
                new ExpressionTokenizer(whereExpression))
            .Parse();

        var predicate =
            new ExpressionBuilder<T>(
                options ?? new ExpressionBuilderOptions())
            .Build(predicateNode);

        return source.Where(predicate);
    }

    public static ICursorQuery<T> AsCursorQuery<T>(this IQueryable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new CursorQuery<T>(source);
    }

    public static Page<T> ToPage<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        var totalItemCount = source.Count();
        var skip = (pageNumber - 1L) * pageSize;
        var items = source
            .Skip((int)skip)
            .Take(pageSize)
            .ToList();

        return new Page<T>(
            items,
            totalItemCount,
            pageNumber,
            pageSize);
    }
}
