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
        Action<Pagination.PaginationCriteriaBuilder<T>> configurePagination)
    {
        configurePagination.ThrowIfNull();

        var pagination = new Pagination.PaginationCriteria<T>();
        configurePagination(new Pagination.PaginationCriteriaBuilder<T>(pagination));

        return query.ToPaginatedList(pagination);
    }
}
