namespace Zift.Pagination.Cursor;

public interface IOrderedCursorQuery<T> : IExecutableCursorQuery<T>
{
    IOrderedCursorQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector);
    IOrderedCursorQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector);

    IExecutableCursorQuery<T> After(string? cursor);
    IExecutableCursorQuery<T> Before(string? cursor);
}
