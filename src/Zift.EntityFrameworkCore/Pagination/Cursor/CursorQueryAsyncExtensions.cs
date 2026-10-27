using Microsoft.EntityFrameworkCore;
using Zift.Pagination.Cursor.Execution;

namespace Zift.Pagination.Cursor;

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
