namespace Zift.Filtering.Dynamic;

public readonly record struct ComparisonOperator(ComparisonOperatorDefinition Definition)
{
    public IReadOnlySet<string> Modifiers { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public bool HasModifier(string modifier) => Modifiers.Contains(modifier);

    public override string ToString()
    {
        return Definition.Symbol.ToLowerInvariant()
            + (Modifiers.Count > 0 ? ":" + string.Join(":", Modifiers) : string.Empty);
    }

    public static bool TryParse(string value, out ComparisonOperator result)
    {
        var parts = value.Split(':');
        var symbol = parts.First();

        if (!ComparisonOperatorDefinitions.TryGet(symbol, out var @operator))
        {
            result = default;
            return false;
        }

        var modifiers = parts
            .Skip(1)
            .Select(m => m.ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!modifiers.All(@operator.SupportedModifiers.Contains))
        {
            result = default;
            return false;
        }

        result = new ComparisonOperator(@operator) { Modifiers = modifiers };
        return true;
    }
}
