namespace Zift.Pagination.Cursor;

using Ordering;

public interface ICursorQuery<T>
{
    IOrderedCursorQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);
    IOrderedCursorQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
    IOrderedCursorQuery<T> OrderBy(string orderByClause, OrderingOptions? options = null);
}
