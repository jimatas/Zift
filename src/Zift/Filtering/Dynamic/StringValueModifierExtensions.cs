namespace Zift.Filtering.Dynamic;

public static class StringValueModifierExtensions
{
    private static readonly Dictionary<StringValueModifier, string> _symbolCache = new()
    {
        [StringValueModifier.IgnoreCase] = "i"
    };

    public static string ToSymbol(this StringValueModifier modifier)
    {
        return _symbolCache.TryGetValue(modifier, out var symbol) ? symbol : modifier.ToString();
    }

    public static StringValueModifier FromSymbol(string symbol)
    {
        foreach (var entry in _symbolCache)
        {
            if (entry.Value.Equals(symbol, StringComparison.OrdinalIgnoreCase))
            {
                return entry.Key;
            }
        }

        throw new ArgumentException($"Unknown string modifier symbol: {symbol}", nameof(symbol));
    }
}
