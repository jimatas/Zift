namespace Zift.Pagination.Offset;

public sealed class Page<T> : IPagedResult<T>
{
    internal Page(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        int totalItemCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        PageCount = CalculatePageCount(totalItemCount, pageSize);
        TotalItemCount = totalItemCount;
    }

    public IReadOnlyList<T> Items { get; }

    public int PageNumber { get; }
    public int PageSize { get; }
    public int PageCount { get; }

    public bool HasNextPage => PageNumber < PageCount;
    public bool HasPreviousPage => PageNumber > 1;

    public int TotalItemCount { get; }

    private static int CalculatePageCount(long totalCount, long pageSize) =>
        (int)((totalCount + pageSize - 1) / pageSize);
}
