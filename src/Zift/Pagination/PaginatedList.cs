namespace Zift.Pagination;

public class PaginatedList<T>(int pageNumber, int pageSize, IReadOnlyList<T> items, int totalCount)
    : IPaginatedList<T>
{
    public static readonly PaginatedList<T> Empty = new(
        pageNumber: 1,
        pageSize: PaginationCriteria<T>.DefaultPageSize,
        items: [],
        totalCount: 0);

    public int PageNumber { get; } = pageNumber.ThrowIfLessThan(1);
    public int PageSize { get; } = pageSize.ThrowIfLessThan(1);
    public int PageCount { get; } = CalculatePageCount(totalCount, pageSize);
    public int TotalCount { get; } = totalCount.ThrowIfLessThan(items?.Count ?? 0);

    public int Count => Items.Count;
    public T this[int index] => Items[index];
    public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected IReadOnlyList<T> Items { get; } = items.ThrowIfNull();

    private static int CalculatePageCount(int totalCount, int pageSize)
    {
        return (int)Math.Ceiling((double)totalCount / pageSize);
    }
}
