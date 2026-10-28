using Zift.Pagination.Cursor.Ordering;

namespace Zift.Pagination.Cursor;

internal sealed record CursorQueryState<T>(
    IReadOnlyList<OrderingClause<T>> Ordering,
    string? Cursor,
    CursorNavigationMode NavigationMode,
    NullOrdering NullOrdering,
    bool EnableNullGuards,
    int MaxOrderingKeys)
{
    public static CursorQueryState<T> Create(NullOrdering nullOrdering, bool enableNullGuards, int maxOrderingKeys)
    {
        return new([], Cursor: null, CursorNavigationMode.None, nullOrdering, enableNullGuards, maxOrderingKeys);
    }
}
