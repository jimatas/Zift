namespace Zift.Pagination.Offset;

public sealed class Page<T> : IPagedResult<T>
{
    internal Page(
        IReadOnlyList<T> items,
        int totalItemCount,
        int pageNumber,
        int pageSize)
    {
        Items = items;
        TotalItemCount = totalItemCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        PageCount = CalculatePageCount();
    }

    public IReadOnlyList<T> Items { get; }
    public int TotalItemCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int PageCount { get; }

    public bool HasNext => PageNumber < PageCount;
    public bool HasPrevious => PageNumber > 1;

    private int CalculatePageCount()
    {
        long totalCount = TotalItemCount;
        long pageSize = PageSize;

        return (int)((totalCount + pageSize - 1) / pageSize);
    }
}
