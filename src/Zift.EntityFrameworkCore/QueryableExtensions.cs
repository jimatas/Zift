namespace Zift;

using Microsoft.EntityFrameworkCore;
using Pagination.Offset;

public static class QueryableExtensions
{
    public static async Task<Page<T>> ToPageAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        var totalItemCount = await source
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var skip = (pageNumber - 1L) * pageSize;
        var items = await source
            .Skip((int)skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new Page<T>(
            items,
            totalItemCount,
            pageNumber,
            pageSize);
    }
}
