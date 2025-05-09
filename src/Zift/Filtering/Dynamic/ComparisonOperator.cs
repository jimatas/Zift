namespace Zift.Filtering.Dynamic;

public readonly record struct ComparisonOperator(ComparisonOperatorType Type)
{
    public IReadOnlySet<string> Modifiers { get; init; } = EmptySet<string>.Instance;

    public bool HasModifier(string modifier) => Modifiers.Contains(modifier);

    public override string ToString()
    {
        return Type + (Modifiers.Count > 0 ? ":" + string.Join(":", Modifiers) : string.Empty);
    }
}
