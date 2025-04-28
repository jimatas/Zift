namespace Zift.EntityFrameworkCore;

public static class QueryablePaginationExtensions
{
    public static async Task<Pagination.IPaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> query,
        Pagination.IPaginationCriteria<T> pagination,
        CancellationToken cancellationToken = default)
    {
        pagination.ThrowIfNull();

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        query = pagination.ApplyTo(query);

        var list = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        return new Pagination.PaginatedList<T>(
            pagination.PageNumber,
            pagination.PageSize,
            list,
            totalCount);
    }

    public static Task<Pagination.IPaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> query,
        Action<Pagination.PaginationCriteriaBuilder<T>> configurePagination,
        CancellationToken cancellationToken = default)
    {
        configurePagination.ThrowIfNull();

        var pagination = new Pagination.PaginationCriteria<T>();
        configurePagination(new Pagination.PaginationCriteriaBuilder<T>(pagination));

        return query.ToPaginatedListAsync(pagination, cancellationToken);
    }
}
