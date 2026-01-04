namespace Zift.Pagination;

public interface IPagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public bool HasNext { get; }
    public bool HasPrevious { get; }
}
