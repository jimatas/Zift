namespace Zift;

public static class QueryableSortingExtensions
{
    public static IQueryable<T> SortBy<T>(this IQueryable<T> query, Sorting.ISortCriteria<T> sortCriteria)
    {
        return sortCriteria.ThrowIfNull().ApplyTo(query);
    }

    public static IQueryable<T> SortBy<T>(this IQueryable<T> query, Action<Sorting.SortCriteriaBuilder<T>> configureSorting)
    {
        configureSorting.ThrowIfNull();
        
        var sortCriteria = new Sorting.SortCriteria<T>();
        configureSorting(new Sorting.SortCriteriaBuilder<T>(sortCriteria));

        return query.SortBy(sortCriteria);
    }
}
