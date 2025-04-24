namespace Zift.EntityFrameworkCore;

public class QueryCriteria<T> : Pagination.PaginationCriteria<T>
{
    public Filtering.IFilterCriteria<T>? Filter { get; set; }
}
