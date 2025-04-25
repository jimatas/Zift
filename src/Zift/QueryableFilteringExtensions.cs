namespace Zift;

public static class QueryableFilteringExtensions
{
    public static IQueryable<T> Filter<T>(this IQueryable<T> query, Filtering.IFilterCriteria<T> filter)
    {
        return filter.ThrowIfNull().ApplyTo(query);
    }
}
