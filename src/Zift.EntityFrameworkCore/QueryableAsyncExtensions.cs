using Microsoft.EntityFrameworkCore;
using Zift.Pagination.Offset;

namespace Zift;

public static class QueryableAsyncExtensions
{
    public static Task<Page<T>> ToPageAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        var offset = OffsetPagination.ComputeOffset(pageNumber, pageSize);

        return ToPageCoreAsync(source, pageNumber, pageSize, offset, cancellationToken);
    }

    private static async Task<Page<T>> ToPageCoreAsync<T>(
        IQueryable<T> source,
        int pageNumber,
        int pageSize,
        int offset,
        CancellationToken cancellationToken)
    {
        var totalItemCount = await source.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await source.Skip(offset).Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false);

        return new Page<T>(items, pageNumber, pageSize, totalItemCount);
    }
}
