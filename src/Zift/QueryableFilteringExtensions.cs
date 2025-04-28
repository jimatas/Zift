namespace Zift;

public static class QueryableFilteringExtensions
{
    public static IQueryable<T> FilterBy<T>(this IQueryable<T> query, Filtering.IFilterCriteria<T> filter)
    {
        return filter.ThrowIfNull().ApplyTo(query);
    }
}
