namespace Zift.Querying.ExpressionBuilding;

public sealed class ExpressionBuilderOptions
{
    public const int DefaultMaxExpressionLength = 4096;
    public const int DefaultMaxNestingDepth = 64;
    public const int DefaultMaxListItems = 1000;

    public bool EnableNullGuards { get; set; } = false;
    public bool ParameterizeValues { get; set; } = true;

    public int MaxExpressionLength
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            field = value;
        }
    } = DefaultMaxExpressionLength;

    public int MaxNestingDepth
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            field = value;
        }
    } = DefaultMaxNestingDepth;

    public int MaxListItems
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            field = value;
        }
    } = DefaultMaxListItems;
}
