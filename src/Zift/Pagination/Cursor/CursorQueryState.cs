namespace Zift.Pagination.Cursor;

using Ordering;

internal sealed record CursorQueryState<T>(
    Ordering<T> Ordering,
    string? Cursor,
    CursorDirection Direction)
{
    public static readonly CursorQueryState<T> Empty = new(
        Ordering<T>.Empty,
        Cursor: null,
        CursorDirection.None);
}
