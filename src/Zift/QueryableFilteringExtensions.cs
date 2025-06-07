namespace Zift;

/// <summary>
/// Extension methods for applying filter criteria to <see cref="IQueryable{T}"/> sources.
/// </summary>
public static class QueryableFilteringExtensions
{
    /// <summary>
    /// Applies the specified filter criteria to the query.
    /// </summary>
    /// <typeparam name="T">The type of elements in the query.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="filter">The filter criteria to apply.</param>
    /// <returns>The filtered query.</returns>
    public static IQueryable<T> Filter<T>(this IQueryable<T> query, Filtering.IFilterCriteria<T> filter)
    {
        return filter.ThrowIfNull().ApplyTo(query);
    }

    /// <summary>
    /// Applies the specified predicate to the query.
    /// </summary>
    /// <typeparam name="T">The type of elements in the query.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="predicate">The predicate to filter by.</param>
    /// <returns>The filtered query.</returns>
    public static IQueryable<T> Filter<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate)
    {
        return query.Filter(new Filtering.PredicateFilterCriteria<T>(predicate));
    }

    /// <summary>
    /// Applies a dynamic filter expression to the query.
    /// </summary>
    /// <typeparam name="T">The type of elements in the query.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="expression">The dynamic filter expression.</param>
    /// <param name="options">Optional filter parsing options.</param>
    /// <returns>The filtered query.</returns>
    public static IQueryable<T> Filter<T>(this IQueryable<T> query, string expression, Filtering.Dynamic.FilterOptions? options = null)
    {
        return query.Filter(new Filtering.DynamicFilterCriteria<T>(expression, options));
    }
}
