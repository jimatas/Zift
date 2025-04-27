namespace Zift;

public static class QueryableFilteringExtensions
{
    public static IQueryable<T> FilterBy<T>(this IQueryable<T> query, Filtering.IFilterCriteria<T> filterCriteria)
    {
        return filterCriteria.ThrowIfNull().ApplyTo(query);
    }
}
