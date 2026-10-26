namespace Zift.Pagination.Cursor;

using Execution;
using Microsoft.EntityFrameworkCore;

public static class CursorQueryAsyncExtensions
{
    public static Task<CursorPage<T>> ToCursorPageAsync<T>(
        this IExecutableCursorQuery<T> query,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return CursorPaginationExecutor<T>.ExecuteAsync(
            query,
            pageSize,
            q => q.ToListAsync(cancellationToken));
    }
}
