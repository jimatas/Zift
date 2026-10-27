namespace Zift.Pagination.Offset;

public sealed class Page<T> : IPagedResult<T>
{
    public Page(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalItemCount)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(totalItemCount);

        if (items.Count > pageSize)
        {
            throw new ArgumentException(
                "A page cannot contain more items than its page size.", nameof(items));
        }

        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalItemCount = totalItemCount;
        PageCount = (int)(((long)totalItemCount + pageSize - 1) / pageSize);
    }

    public static Page<T> Empty(int pageSize)
    {
        return new Page<T>([], pageNumber: 1, pageSize, totalItemCount: 0);
    }

    public IReadOnlyList<T> Items { get; }

    public int PageNumber { get; }
    public int PageSize { get; }
    public int PageCount { get; }

    public bool HasNextPage => PageNumber < PageCount;
    public bool HasPreviousPage => PageNumber > 1;

    public int TotalItemCount { get; }

    public Page<TResult> Map<TResult>(Func<T, TResult> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        return new Page<TResult>(Items.Select(selector).ToList(), PageNumber, PageSize, TotalItemCount);
    }
}
