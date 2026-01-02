namespace Zift.Pagination.Cursor;

public interface ICursorQuery<T>
{
    IOrderedCursorQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);
    IOrderedCursorQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
    IOrderedCursorQuery<T> OrderBy(string orderByClause);
}
