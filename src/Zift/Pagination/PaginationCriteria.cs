namespace Zift.Pagination;

/// <summary>
/// Default implementation of <see cref="IPaginationCriteria{T}"/>.
/// </summary>
/// <typeparam name="T">The type of elements in the query.</typeparam>
public class PaginationCriteria<T> : IPaginationCriteria<T>
{
    /// <summary>
    /// The default number of items per page.
    /// </summary>
    public const int DefaultPageSize = 20;

    private int _pageNumber;
    private int _pageSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginationCriteria{T}"/> class.
    /// </summary>
    /// <param name="pageNumber">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    public PaginationCriteria(int pageNumber = 1, int pageSize = DefaultPageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <inheritdoc/>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value.ThrowIfLessThan(1, nameof(PageNumber));
    }

    /// <inheritdoc/>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value.ThrowIfLessThan(1, nameof(PageSize));
    }

    /// <inheritdoc/>
    public IQueryable<T> ApplyTo(IQueryable<T> query)
    {
        query.ThrowIfNull();

        query = query.Skip((PageNumber - 1) * PageSize).Take(PageSize);

        return query;
    }
}
