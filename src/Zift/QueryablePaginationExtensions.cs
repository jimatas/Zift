namespace Zift;

/// <summary>
/// Extension methods for applying pagination to <see cref="IQueryable{T}"/> sources.
/// </summary>
public static class QueryablePaginationExtensions
{
    /// <summary>
    /// Converts the query to a paginated list using the specified pagination criteria.
    /// </summary>
    /// <typeparam name="T">The type of elements in the query.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="pagination">The pagination criteria to apply.</param>
    /// <returns>A paginated list of results.</returns>
    public static Pagination.IPaginatedList<T> ToPaginatedList<T>(
        this IQueryable<T> query,
        Pagination.IPaginationCriteria<T> pagination)
    {
        pagination.ThrowIfNull();

        var totalCount = query.Count();

        query = pagination.ApplyTo(query);

        var list = query.ToList();

        return new Pagination.PaginatedList<T>(
            pagination.PageNumber,
            pagination.PageSize,
            list,
            Math.Max(totalCount, list.Count)); // Correct for stale count between count & fetch.
    }

    /// <summary>
    /// Converts the query to a paginated list using the given page number and page size.
    /// </summary>
    /// <typeparam name="T">The type of elements in the query.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="pageNumber">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paginated list of results.</returns>
    public static Pagination.IPaginatedList<T> ToPaginatedList<T>(
        this IQueryable<T> query,
        int pageNumber = 1,
        int pageSize = Pagination.PaginationCriteria<T>.DefaultPageSize) =>
        query.ToPaginatedList(new Pagination.PaginationCriteria<T>(pageNumber, pageSize));
}
