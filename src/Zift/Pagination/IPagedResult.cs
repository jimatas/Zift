namespace Zift.Pagination;

public interface IPagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public bool HasNextPage { get; }
    public bool HasPreviousPage { get; }
}
