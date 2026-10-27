using Zift.Pagination.Cursor.Execution;

namespace Zift.Pagination.Cursor;

public static class CursorQueryExtensions
{
    public static CursorPage<T> ToCursorPage<T>(
        this IExecutableCursorQuery<T> query,
        int pageSize)
    {
        return CursorPaginationExecutor<T>.Execute(
            query,
            pageSize,
            static q => q.ToList());
    }
}
