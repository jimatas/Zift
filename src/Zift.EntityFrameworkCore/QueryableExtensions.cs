namespace Zift.EntityFrameworkCore;

public static class QueryableExtensions
{
    public static async Task<Pagination.IPaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> query,
        Action<QueryCriteriaBuilder<T>> configureQuery,
        CancellationToken cancellationToken = default)
    {
        configureQuery.ThrowIfNull();

        var queryCriteria = new QueryCriteria<T>();
        var queryCriteriaBuilder = new QueryCriteriaBuilder<T>(queryCriteria);

        configureQuery(queryCriteriaBuilder);
        
        if (queryCriteria.Filter is { } filterCriteria)
        {
            query = filterCriteria.ApplyTo(query);
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var paginationCriteria = queryCriteria as Pagination.IPaginationCriteria<T>;
        query = paginationCriteria.ApplyTo(query);

        var list = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        return new Pagination.PaginatedList<T>(
            paginationCriteria.PageNumber,
            paginationCriteria.PageSize,
            list,
            totalCount);
    }
}
