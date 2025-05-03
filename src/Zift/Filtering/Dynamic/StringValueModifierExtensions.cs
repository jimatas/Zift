namespace Zift.Filtering.Dynamic;

public static class StringValueModifierExtensions
{
    private static readonly Dictionary<StringValueModifier, string> _symbolMap = new()
    {
        [StringValueModifier.IgnoreCase] = "i"
    };

    public static string ToSymbol(this StringValueModifier modifier)
    {
        return _symbolMap.TryGetValue(modifier, out var symbol) ? symbol : modifier.ToString();
    }

    public static StringValueModifier FromSymbol(string symbol)
    {
        foreach (var (modifier, mappedSymbol) in _symbolMap)
        {
            if (mappedSymbol.Equals(symbol, StringComparison.OrdinalIgnoreCase))
            {
                return modifier;
            }
        }

        throw new ArgumentException($"Unknown string modifier: {symbol}", nameof(symbol));
    }
}
