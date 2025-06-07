namespace Zift.Pagination;

/// <summary>
/// A read-only list that includes pagination metadata.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
public interface IPaginatedList<T> : IReadOnlyList<T>
{
    /// <summary>
    /// The 1-based page number.
    /// </summary>
    int PageNumber { get; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    int PageCount { get; }

    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    int TotalCount { get; }
}
