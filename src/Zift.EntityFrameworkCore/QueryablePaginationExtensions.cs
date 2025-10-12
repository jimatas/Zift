namespace Zift.EntityFrameworkCore;

/// <summary>
/// Entity Framework Core extension methods for applying pagination to <see cref="IQueryable{T}"/> sources.
/// </summary>
public static class QueryablePaginationExtensions
{
    /// <summary>
    /// Asynchronously converts the query to a paginated list using the specified pagination criteria.
    /// </summary>
    /// <typeparam name="T">The type of elements in the query.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="pagination">The pagination criteria to apply.</param>
    /// <param name="cancellationToken">A token for canceling the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains a paginated list of results.</returns>
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
            Math.Max(totalCount, list.Count)); // Correct for stale count between count & fetch.
    }

    /// <summary>
    /// Asynchronously converts the query to a paginated list using the given page number and page size.
    /// </summary>
    /// <typeparam name="T">The type of elements in the query.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="pageNumber">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token for canceling the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains a paginated list of results.</returns>
    public static Task<Pagination.IPaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> query,
        int pageNumber = 1,
        int pageSize = Pagination.PaginationCriteria<T>.DefaultPageSize,
        CancellationToken cancellationToken = default) =>
        query.ToPaginatedListAsync(
            new Pagination.PaginationCriteria<T>(pageNumber, pageSize),
            cancellationToken);
}
