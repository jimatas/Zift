namespace Zift.Pagination;

/// <summary>
/// Represents pagination criteria that can be applied to a query.
/// </summary>
/// <typeparam name="T">The type of elements in the query.</typeparam>
public interface IPaginationCriteria<T> : ICriteria<T>
{
    /// <summary>
    /// The 1-based page number.
    /// </summary>
    int PageNumber { get; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    int PageSize { get; }
}
