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

    public IList<Sorting.ISortCriterion<T>> SortCriteria { get; } = [];
    IReadOnlyList<Sorting.ISortCriterion<T>> IPaginationCriteria<T>.SortCriteria => SortCriteria.ToArray();

    public IQueryable<T> ApplyTo(IQueryable<T> query)
    {
        query.ThrowIfNull();

        query = ApplySorting(query);
        query = ApplyPagination(query);

        return query;
    }

    private IQueryable<T> ApplySorting(IQueryable<T> query)
    {
        IOrderedQueryable<T>? sortedQuery = null;

        foreach (var criterion in SortCriteria)
        {
            sortedQuery = sortedQuery is null
                ? criterion.ApplyTo(query)
                : criterion.ApplyTo(sortedQuery);
        }

        return sortedQuery ?? query;
    }

    private IQueryable<T> ApplyPagination(IQueryable<T> query)
    {
        return query.Skip((PageNumber - 1) * PageSize).Take(PageSize);
    }
}
