namespace Zift;

public static class QueryablePaginationExtensions
{
    public static Pagination.IPaginatedList<T> ToPaginatedList<T>(this IQueryable<T> query,
        Action<Pagination.PaginationCriteriaBuilder<T>> configurePagination)
    {
        configurePagination.ThrowIfNull();

        var paginationCriteria = new Pagination.PaginationCriteria<T>();
        configurePagination(new Pagination.PaginationCriteriaBuilder<T>(paginationCriteria));

        return query.ToPaginatedList(paginationCriteria);
    }

    public static Pagination.IPaginatedList<T> ToPaginatedList<T>(this IQueryable<T> query,
        Pagination.IPaginationCriteria<T> paginationCriteria)
    {
        paginationCriteria.ThrowIfNull();

        var totalCount = query.Count();

        query = paginationCriteria.ApplyTo(query);

        var list = query.ToList();

        return new Pagination.PaginatedList<T>(
            paginationCriteria.PageNumber,
            paginationCriteria.PageSize,
            list,
            totalCount);
    }
}
