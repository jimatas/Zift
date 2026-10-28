namespace Zift.Pagination.Cursor;

public sealed class CursorQueryOptions
{
    public const int DefaultMaxOrderingKeys = 16;

    public NullOrdering NullOrdering
    {
        get;
        set
        {
            if (!Enum.IsDefined(value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The null ordering is not a defined value.");
            }

            field = value;
        }
    } = NullOrdering.NullsCompareLower;

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
