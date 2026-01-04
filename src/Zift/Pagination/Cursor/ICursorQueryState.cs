namespace Zift.Pagination.Cursor;

internal interface ICursorQueryState<T>
{
    public IQueryable<T> Source { get; }
    public CursorQueryState<T> State { get; }
}
