namespace Zift.Pagination;

public static class PaginatedListExtensions
{
    public static bool IsFirstPage<T>(this IPaginatedList<T> paginatedList)
    {
        return paginatedList.PageNumber == 1;
    }

    public static bool IsLastPage<T>(this IPaginatedList<T> paginatedList)
    {
        return paginatedList.PageNumber >= paginatedList.PageCount;
    }

    public static bool HasNextPage<T>(this IPaginatedList<T> paginatedList)
    {
        return paginatedList.PageNumber < paginatedList.PageCount;
    }

    public static bool HasPreviousPage<T>(this IPaginatedList<T> paginatedList)
    {
        return paginatedList.PageNumber > 1;
    }
}
