namespace Zift.Pagination;

public class PaginationCriteriaBuilder<T>(PaginationCriteria<T> criteria)
{
    private readonly PaginationCriteria<T> _criteria = criteria.ThrowIfNull();

    public PaginationCriteriaBuilder<T> StartAt(int pageNumber)
    {
        _criteria.PageNumber = pageNumber;

        return this;
    }

    public PaginationCriteriaBuilder<T> WithPageSize(int pageSize)
    {
        _criteria.PageSize = pageSize;

        return this;
    }
}
