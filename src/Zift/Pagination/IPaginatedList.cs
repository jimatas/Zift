namespace Zift.Pagination;

public interface IPaginatedList<T> : IReadOnlyList<T>
{
    int PageNumber { get; }
    int PageSize { get; }
    int PageCount { get; }
    int TotalCount { get; }
}
