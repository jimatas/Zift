namespace Zift.Filtering.Dynamic;

public readonly record struct LiteralValue(object? RawValue)
{
    public StringValueModifier? Modifier { get; init; }

    public bool HasModifier(StringValueModifier expected)
    {
        return Modifier.HasValue && Modifier.Value == expected;
    }

    public override string ToString()
    {
        return RawValue switch
        {
            null => "null",
            bool b => b ? "true" : "false",
            string s when Modifier is { } modifier => $"{s}:{modifier.ToDisplayString()}",
            double d => d.ToString("R", CultureInfo.InvariantCulture),
            _ => RawValue.ToString()!,
        };
    }
}
