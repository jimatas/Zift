namespace Zift.Querying.Model;

internal sealed record NumberLiteral : LiteralNode
{
    public NumberLiteral(int value) => Value = value;
    public NumberLiteral(double value) => Value = value;

    public object Value { get; }
}
