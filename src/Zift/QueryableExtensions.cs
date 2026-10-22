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
        ArgumentException.ThrowIfNullOrWhiteSpace(whereExpression);

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

    public static ICursorQuery<T> AsCursorQuery<T>(
        this IQueryable<T> source,
        CursorQueryOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        var nullOrdering = options?.NullOrdering ?? NullOrdering.NullsCompareLower;
        if (!Enum.IsDefined(nullOrdering))
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                nullOrdering,
                "The null ordering is not a defined value.");
        }

        return new CursorQuery<T>(source, nullOrdering);
    }

    public static Page<T> ToPage<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        var offset = OffsetPagination.ComputeOffset(pageNumber, pageSize);

        var totalItemCount = source.Count();
        var items = source
            .Skip(offset)
            .Take(pageSize)
            .ToList();

        return new Page<T>(
            items,
            pageNumber,
            pageSize,
            totalItemCount);
    }
}
