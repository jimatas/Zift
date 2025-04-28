namespace Zift.EntityFrameworkCore;

public static class QueryablePaginationExtensions
{
    public static async Task<Pagination.IPaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> query,
        Pagination.IPaginationCriteria<T> paginationCriteria,
        CancellationToken cancellationToken = default)
    {
        paginationCriteria.ThrowIfNull();

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        query = paginationCriteria.ApplyTo(query);

        var list = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        return new Pagination.PaginatedList<T>(
            paginationCriteria.PageNumber,
            paginationCriteria.PageSize,
            list,
            totalCount);
    }

    public static Task<Pagination.IPaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> query,
        Action<Pagination.PaginationCriteriaBuilder<T>> configurePagination,
        CancellationToken cancellationToken = default)
    {
        configurePagination.ThrowIfNull();

        var paginationCriteria = new Pagination.PaginationCriteria<T>();
        configurePagination(new Pagination.PaginationCriteriaBuilder<T>(paginationCriteria));

        return query.ToPaginatedListAsync(paginationCriteria, cancellationToken);
    }
}
