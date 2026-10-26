namespace Zift.Filtering.Model;

internal sealed record NumberLiteral(string Text) : LiteralNode
{
    public NumberLiteral(int value)
        : this(value.ToString(CultureInfo.InvariantCulture))
    {
    }

    public NumberLiteral(double value)
        : this(value.ToString("R", CultureInfo.InvariantCulture))
    {
    }
}
