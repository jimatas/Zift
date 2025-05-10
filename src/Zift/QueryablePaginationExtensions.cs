namespace Zift;

public static class QueryablePaginationExtensions
{
    public static Pagination.IPaginatedList<T> ToPaginatedList<T>(this IQueryable<T> query,
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
            totalCount);
    }

    public static Pagination.IPaginatedList<T> ToPaginatedList<T>(this IQueryable<T> query,
        int pageNumber = 1,
        int pageSize = Pagination.PaginationCriteria<T>.DefaultPageSize)
    {
        return query.ToPaginatedList(new Pagination.PaginationCriteria<T>(pageNumber, pageSize));
    }
}
