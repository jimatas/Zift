namespace Zift;

public static class QueryableFilteringExtensions
{
    public static IQueryable<T> Filter<T>(this IQueryable<T> query, Filtering.IFilterCriteria<T> filter)
    {
        return filter.ThrowIfNull().ApplyTo(query);
    }

    public static IQueryable<T> Filter<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate)
    {
        return query.Filter(new Filtering.PredicateFilterCriteria<T>(predicate));
    }

    public static IQueryable<T> Filter<T>(this IQueryable<T> query, string expression, Filtering.Dynamic.FilterOptions? options = null)
    {
        return query.Filter(new Filtering.DynamicFilterCriteria<T>(expression, options));
    }
}
