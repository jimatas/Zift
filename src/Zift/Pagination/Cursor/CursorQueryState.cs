namespace Zift.Pagination.Cursor;

using Ordering;

internal sealed record CursorQueryState<T>(
    IReadOnlyList<OrderingClause<T>> Ordering,
    string? Cursor,
    CursorNavigationMode NavigationMode,
    NullOrdering NullOrdering)
{
    public static CursorQueryState<T> Create(NullOrdering nullOrdering)
    {
        return new([], Cursor: null, CursorNavigationMode.None, nullOrdering);
    }
}
