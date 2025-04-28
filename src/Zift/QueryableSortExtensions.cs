namespace Zift;

public static class QueryableSortExtensions
{
    public static IQueryable<T> SortBy<T>(this IQueryable<T> query, Sorting.ISortCriteria<T> sort)
    {
        return sort.ThrowIfNull().ApplyTo(query);
    }

    public static IQueryable<T> SortBy<T>(this IQueryable<T> query, Action<Sorting.SortCriteriaBuilder<T>> configureSort)
    {
        configureSort.ThrowIfNull();
        
        var sortCriteria = new Sorting.SortCriteria<T>();
        configureSort(new Sorting.SortCriteriaBuilder<T>(sortCriteria));

        return query.SortBy(sortCriteria);
    }
}
