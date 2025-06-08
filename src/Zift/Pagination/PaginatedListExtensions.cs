namespace Zift.Pagination;

/// <summary>
/// Extension methods for <see cref="IPaginatedList{T}"/>.
/// </summary>
public static class PaginatedListExtensions
{
    /// <summary>
    /// Determines whether the current page is the first page.
    /// </summary>
    /// <typeparam name="T">The type of elements in the paginated list.</typeparam>
    /// <param name="paginatedList">The paginated list to check.</param>
    /// <returns><see langword="true"/> if the current page is the first page; otherwise, <see langword="false"/>.</returns>
    public static bool IsFirstPage<T>(this IPaginatedList<T> paginatedList)
    {
        return paginatedList.PageNumber == 1;
    }

    /// <summary>
    /// Determines whether the current page is the last page.
    /// </summary>
    /// <typeparam name="T">The type of elements in the paginated list.</typeparam>
    /// <param name="paginatedList">The paginated list to check.</param>
    /// <returns><see langword="true"/> if the current page is the last page; otherwise, <see langword="false"/>.</returns>
    public static bool IsLastPage<T>(this IPaginatedList<T> paginatedList)
    {
        return paginatedList.PageNumber >= paginatedList.PageCount;
    }

    /// <summary>
    /// Determines whether there is a next page.
    /// </summary>
    /// <typeparam name="T">The type of elements in the paginated list.</typeparam>
    /// <param name="paginatedList">The paginated list to check.</param>
    /// <returns><see langword="true"/> if there is a next page; otherwise, <see langword="false"/>.</returns>
    public static bool HasNextPage<T>(this IPaginatedList<T> paginatedList)
    {
        return paginatedList.PageNumber < paginatedList.PageCount;
    }

    /// <summary>
    /// Determines whether there is a previous page.
    /// </summary>
    /// <typeparam name="T">The type of elements in the paginated list.</typeparam>
    /// <param name="paginatedList">The paginated list to check.</param>
    /// <returns><see langword="true"/> if there is a previous page; otherwise, <see langword="false"/>.</returns>
    public static bool HasPreviousPage<T>(this IPaginatedList<T> paginatedList)
    {
        return paginatedList.PageNumber > 1;
    }
}
