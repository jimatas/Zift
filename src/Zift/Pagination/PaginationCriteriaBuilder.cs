namespace Zift.Pagination;

public class PaginationCriteriaBuilder<T>(PaginationCriteria<T> paginationCriteria)
{
    private readonly PaginationCriteria<T> _paginationCriteria = paginationCriteria.ThrowIfNull();

    public PaginationCriteriaBuilder<T> StartAt(int pageNumber)
    {
        _paginationCriteria.PageNumber = pageNumber;

        return this;
    }

    public PaginationCriteriaBuilder<T> WithPageSize(int pageSize)
    {
        _paginationCriteria.PageSize = pageSize;

        return this;
    }
}
