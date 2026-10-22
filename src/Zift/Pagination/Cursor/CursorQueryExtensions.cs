namespace Zift.Pagination.Cursor;

using Execution;

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
