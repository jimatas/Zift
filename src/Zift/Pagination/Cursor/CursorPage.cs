namespace Zift.Pagination.Cursor;

public sealed class CursorPage<T> : IPagedResult<T>
{
    internal CursorPage(
        IReadOnlyList<T> items,
        string? startCursor,
        string? endCursor,
        bool hasNextPage,
        bool hasPreviousPage)
    {
        Items = items;
        StartCursor = startCursor;
        EndCursor = endCursor;
        HasNextPage = hasNextPage;
        HasPreviousPage = hasPreviousPage;
    }

    public IReadOnlyList<T> Items { get; }

    public string? StartCursor { get; }
    public string? EndCursor { get; }

    [MemberNotNullWhen(true, nameof(EndCursor))]
    public bool HasNextPage { get; }

    [MemberNotNullWhen(true, nameof(StartCursor))]
    public bool HasPreviousPage { get; }
}
