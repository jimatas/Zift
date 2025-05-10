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
        int pageNumber = 1,
        int pageSize = Pagination.PaginationCriteria<T>.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        return query.ToPaginatedListAsync(
            new Pagination.PaginationCriteria<T>(pageNumber, pageSize),
            cancellationToken);
    }
}
