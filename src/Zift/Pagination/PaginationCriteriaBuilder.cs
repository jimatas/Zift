namespace Zift.Pagination;

public class PaginationCriteriaBuilder<T>(PaginationCriteria<T> paginationCriteria)
{
    private readonly PaginationCriteria<T> _paginationCriteria = paginationCriteria;

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

    public PaginationCriteriaBuilder<T> SortBy(string property,
        Sorting.SortDirection direction = Sorting.SortDirection.Ascending)
    {
        _paginationCriteria.SortCriteria.Add(new Sorting.SortCriterion<T>(property, direction));

        return this;
    }

    public PaginationCriteriaBuilder<T> SortBy<TProperty>(Expression<Func<T, TProperty>> property,
        Sorting.SortDirection direction = Sorting.SortDirection.Ascending)
    {
        _paginationCriteria.SortCriteria.Add(new Sorting.SortCriterion<T, TProperty>(property, direction));

        return this;
    }

    public PaginationCriteriaBuilder<T> SortBy(string sortString,
        Sorting.Dynamic.ISortDirectiveParser<T> parser)
    {
        parser.ThrowIfNull();

        foreach (var criterion in parser.Parse(sortString))
        {
            _paginationCriteria.SortCriteria.Add(criterion);
        }

        return this;
    }
}
