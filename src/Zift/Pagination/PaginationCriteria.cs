namespace Zift.Pagination;

public class PaginationCriteria<T> : IPaginationCriteria<T>
{
    public const int DefaultPageSize = 20;

    private int _pageNumber;
    private int _pageSize;

    public PaginationCriteria(int pageNumber = 1, int pageSize = DefaultPageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value.ThrowIfLessThan(1, nameof(PageNumber));
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value.ThrowIfLessThan(1, nameof(PageSize));
    }

    public IQueryable<T> ApplyTo(IQueryable<T> query)
    {
        query.ThrowIfNull();

        query = query.Skip((PageNumber - 1) * PageSize).Take(PageSize);

        return query;
    }
}
