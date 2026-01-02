namespace Zift.Pagination.Cursor;

using Execution;

public static class CursorQueryExtensions
{
    public static CursorPage<T> ToCursorPage<T>(
        this IExecutableCursorQuery<T> query,
        int pageSize)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        if (query is not ICursorQueryState<T> { Source: var source, State: var state })
        {
            throw new InvalidOperationException(
                "Cursor query does not provide required execution state.");
        }

        return CursorPaginationExecutor<T>.Execute(
            source,
            state,
            pageSize,
            static (q, limit) => q.Take(limit).ToList());
    }
}
