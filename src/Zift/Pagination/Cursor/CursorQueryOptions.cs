namespace Zift.Pagination.Cursor;

public sealed class CursorQueryOptions
{
    public const int DefaultMaxOrderingKeys = 16;

    public NullOrdering NullOrdering { get; set; } = NullOrdering.NullsCompareLower;

    public bool EnableNullGuards { get; set; } = false;

    public int MaxOrderingKeys
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            field = value;
        }
    } = DefaultMaxOrderingKeys;
}
