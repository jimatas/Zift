namespace Zift.Pagination.Offset;

internal static class OffsetPagination
{
    public static int ComputeOffset(int pageNumber, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        var offset = (pageNumber - 1L) * pageSize;
        if (offset > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                pageNumber,
                "The requested page lies beyond the maximum supported offset.");
        }

        return (int)offset;
    }
}
