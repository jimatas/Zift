namespace Zift.Pagination.Cursor;

using Ordering;

internal sealed record CursorQueryState<T>(
    IReadOnlyList<OrderingClause<T>> Ordering,
    string? Cursor,
    CursorNavigationMode NavigationMode,
    NullOrdering NullOrdering,
    int MaxOrderingKeys)
{
    public static CursorQueryState<T> Create(NullOrdering nullOrdering, int maxOrderingKeys)
    {
        return new([], Cursor: null, CursorNavigationMode.None, nullOrdering, maxOrderingKeys);
    }
}
