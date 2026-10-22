namespace Zift.Pagination.Cursor;

public sealed class CursorQueryOptions
{
    public NullOrdering NullOrdering { get; set; } = NullOrdering.NullsCompareLower;
}
