namespace Zift.Pagination;

public interface IPaginationCriteria<T> : ICriteria<T>
{
    int PageNumber { get; }
    int PageSize { get; }
    IReadOnlyList<Sorting.ISortCriterion<T>> SortCriteria { get; }
}
