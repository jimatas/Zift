namespace Zift.Filtering.Dynamic;

public readonly record struct LiteralValue(object? RawValue)
{
    public string? Modifier { get; init; }

    public bool HasModifier(string expected)
    {
        return string.Equals(Modifier, expected, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString()
    {
        return RawValue switch
        {
            null => "null",
            bool b => b ? "true" : "false",
            string s when Modifier is { } modifier => $"{s}:{modifier}",
            double d => d.ToString("R", CultureInfo.InvariantCulture),
            _ => RawValue.ToString()!,
        };
    }
}
