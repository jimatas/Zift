namespace Zift.Pagination.Cursor;

using Execution;
using Microsoft.EntityFrameworkCore;

public static class CursorQueryExtensions
{
    public static Task<CursorPage<T>> ToCursorPageAsync<T>(
        this IExecutableCursorQuery<T> query,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        if (query is not ICursorQueryState<T> { Source: var source, State: var state })
        {
            throw new InvalidOperationException(
                "Cursor query does not provide required execution state.");
        }

        return CursorPaginationExecutor<T>.ExecuteAsync(
            source,
            state,
            pageSize,
            static (q, limit, ct) => q.Take(limit).ToListAsync(ct),
            cancellationToken);
    }
}
