namespace Zift.Filtering.Dynamic;

public static class QuantifierModeExtensions
{
    private static readonly ConcurrentDictionary<(string, int), MethodInfo> _linqMethodCache = new();
    private static readonly Dictionary<QuantifierMode, string> _symbolMap = new()
    {
        [QuantifierMode.Any] = "any",
        [QuantifierMode.All] = "all"
    };

    public static string ToSymbol(this QuantifierMode quantifier)
    {
        return _symbolMap.TryGetValue(quantifier, out var symbol) ? symbol : quantifier.ToString();
    }

    public static QuantifierMode FromSymbol(string symbol)
    {
        foreach (var (quantifier, mappedSymbol) in _symbolMap)
        {
            if (mappedSymbol.Equals(symbol, StringComparison.OrdinalIgnoreCase))
            {
                return quantifier;
            }
        }

        throw new ArgumentException($"Unknown quantifier mode: {symbol}", nameof(symbol));
    }

    public static MethodInfo ToLinqMethod(this QuantifierMode quantifier, bool withPredicate)
    {
        return _linqMethodCache.GetOrAdd(
            key: (quantifier.ToString(), withPredicate ? 2 : 1),
            valueFactory: ResolveLinqMethod);
    }

    private static MethodInfo ResolveLinqMethod((string MethodName, int ParameterCount) signature)
    {
        return typeof(Enumerable)
            .GetMethods()
            .Single(method =>
                method.Name == signature.MethodName
                && method.GetParameters().Length == signature.ParameterCount);
    }
}
