namespace Zift.Pagination;

/// <summary>
/// Default implementation of <see cref="IPaginatedList{T}"/>.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
/// <param name="pageNumber">The 1-based page number.</param>
/// <param name="pageSize">The number of items per page.</param>
/// <param name="items">A read-only list of items on the current page.</param>
/// <param name="totalCount">The total number of items across all pages.</param>
public class PaginatedList<T>(int pageNumber, int pageSize, IReadOnlyList<T> items, int totalCount)
    : IPaginatedList<T>
{
    /// <summary>
    /// An empty paginated list with default page size and no items.
    /// </summary>
    public static readonly PaginatedList<T> Empty = new(
        pageNumber: 1,
        pageSize: PaginationCriteria<T>.DefaultPageSize,
        items: [],
        totalCount: 0);

    /// <inheritdoc/>
    public int PageNumber { get; } = pageNumber.ThrowIfLessThan(1);

    /// <inheritdoc/>
    public int PageSize { get; } = pageSize.ThrowIfLessThan(1);

    /// <inheritdoc/>
    public int PageCount { get; } = CalculatePageCount(totalCount, pageSize);

    /// <inheritdoc/>
    public int TotalCount { get; } = totalCount.ThrowIfLessThan(items?.Count ?? 0);

    /// <inheritdoc/>
    public int Count => Items.Count;

    /// <inheritdoc/>
    public T this[int index] => Items[index];

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// A read-only list of items on the current page.
    /// </summary>
    protected IReadOnlyList<T> Items { get; } = items.ThrowIfNull();

    private static int CalculatePageCount(int totalCount, int pageSize)
    {
        return (int)Math.Ceiling((double)totalCount / pageSize);
    }
}
