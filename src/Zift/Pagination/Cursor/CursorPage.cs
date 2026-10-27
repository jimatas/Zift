namespace Zift.Pagination.Cursor;

public sealed class CursorPage<T> : IPagedResult<T>
{
    public CursorPage(
        IReadOnlyList<T> items,
        string? startCursor,
        string? endCursor,
        bool hasNextPage,
        bool hasPreviousPage)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (hasNextPage && endCursor is null)
        {
            throw new ArgumentException("A next page requires an end cursor.", nameof(endCursor));
        }

        if (hasPreviousPage && startCursor is null)
        {
            throw new ArgumentException("A previous page requires a start cursor.", nameof(startCursor));
        }

        if (items.Count == 0)
        {
            if (startCursor is not null || endCursor is not null || hasNextPage || hasPreviousPage)
            {
                throw new ArgumentException(
                    "An empty page cannot have cursors or continuation.", nameof(items));
            }
        }
        else
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(startCursor);
            ArgumentException.ThrowIfNullOrWhiteSpace(endCursor);
        }

        Items = items;
        StartCursor = startCursor;
        EndCursor = endCursor;
        HasNextPage = hasNextPage;
        HasPreviousPage = hasPreviousPage;
    }

    public static CursorPage<T> Empty { get; } = new([], null, null, hasNextPage: false, hasPreviousPage: false);

    public IReadOnlyList<T> Items { get; }

    public string? StartCursor { get; }
    public string? EndCursor { get; }

    [MemberNotNullWhen(true, nameof(EndCursor))]
    public bool HasNextPage { get; }

    [MemberNotNullWhen(true, nameof(StartCursor))]
    public bool HasPreviousPage { get; }

    public CursorPage<TResult> Map<TResult>(Func<T, TResult> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        return new CursorPage<TResult>(
            Items.Select(selector).ToList(),
            StartCursor,
            EndCursor,
            HasNextPage,
            HasPreviousPage);
    }
}
