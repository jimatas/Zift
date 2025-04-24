namespace Zift.EntityFrameworkCore;

public class QueryCriteriaBuilder<T>(QueryCriteria<T> queryCriteria)
{
    public QueryCriteria<T> QueryCriteria { get; } = queryCriteria.ThrowIfNull();

    public QueryCriteriaBuilder<T> FilterBy(Filtering.IFilterCriteria<T> filter)
    {
        QueryCriteria.Filter = filter.ThrowIfNull();

        return this;
    }

    public virtual QueryCriteriaBuilder<T> PaginateBy(int pageNumber)
    {
        QueryCriteria.PageNumber = pageNumber;

        return this;
    }

    public virtual QueryCriteriaBuilder<T> PaginateBy(int pageNumber, int pageSize)
    {
        QueryCriteria.PageNumber = pageNumber;
        QueryCriteria.PageSize = pageSize;

        return this;
    }

    public virtual QueryCriteriaBuilder<T> SortBy(Sorting.ISortCriterion<T> criterion)
    {
        QueryCriteria.SortCriteria.Add(criterion);

        return this;
    }

    public virtual QueryCriteriaBuilder<T> SortBy<TProperty>(Sorting.SortCriterion<T, TProperty> criterion)
    {
        QueryCriteria.SortCriteria.Add(criterion);

        return this;
    }
}
