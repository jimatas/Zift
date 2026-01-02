namespace Zift.Pagination.Cursor;

public sealed class CursorPage<T> : IPagedResult<T>
{
    internal CursorPage(
        IReadOnlyList<T> items,
        string? nextCursor,
        string? previousCursor)
    {
        Items = items;
        NextCursor = nextCursor;
        PreviousCursor = previousCursor;
    }

    public IReadOnlyList<T> Items { get; }

    public string? NextCursor { get; }
    public string? PreviousCursor { get; }

    [MemberNotNullWhen(true, nameof(NextCursor))]
    public bool HasNext => NextCursor is not null;

    [MemberNotNullWhen(true, nameof(PreviousCursor))]
    public bool HasPrevious => PreviousCursor is not null;
}
