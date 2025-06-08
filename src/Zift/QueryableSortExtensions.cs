namespace Zift;

/// <summary>
/// Extension methods for applying sort criteria to <see cref="IQueryable{T}"/> sources.
/// </summary>
public static class QueryableSortExtensions
{
    /// <summary>
    /// Applies the specified sort criteria to the query.
    /// </summary>
    /// <typeparam name="T">The type of elements in the query.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="sort">The sort criteria to apply.</param>
    /// <returns>The sorted query.</returns>
    public static IQueryable<T> SortBy<T>(this IQueryable<T> query, Sorting.ISortCriteria<T> sort)
    {
        return sort.ThrowIfNull().ApplyTo(query);
    }

    /// <summary>
    /// Applies sort criteria to the query using a configuration delegate.
    /// </summary>
    /// <typeparam name="T">The type of elements in the query.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="configureSort">The delegate used to configure sort criteria.</param>
    /// <returns>The sorted query.</returns>
    public static IQueryable<T> SortBy<T>(this IQueryable<T> query, Action<Sorting.SortCriteriaBuilder<T>> configureSort)
    {
        configureSort.ThrowIfNull();

        var sort = new Sorting.SortCriteria<T>();
        configureSort(new Sorting.SortCriteriaBuilder<T>(sort));

        return query.SortBy(sort);
    }
}
