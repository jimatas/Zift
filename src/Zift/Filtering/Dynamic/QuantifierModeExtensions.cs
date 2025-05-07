namespace Zift.Filtering.Dynamic;

public static class QuantifierModeExtensions
{
    private static readonly Dictionary<QuantifierMode, string> _symbols = new()
    {
        [QuantifierMode.Any] = "any",
        [QuantifierMode.All] = "all"
    };

    private static readonly Dictionary<string, QuantifierMode> _bySymbol = _symbols.ToDictionary(
        kvp => kvp.Value,
        kvp => kvp.Key,
        StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<(QuantifierMode, int), MethodInfo> _linqMethods = new()
    {
        [(QuantifierMode.Any, 1)] = ResolveLinqMethod(nameof(Enumerable.Any), 1),
        [(QuantifierMode.Any, 2)] = ResolveLinqMethod(nameof(Enumerable.Any), 2),
        [(QuantifierMode.All, 2)] = ResolveLinqMethod(nameof(Enumerable.All), 2)
    };

    public static string ToSymbol(this QuantifierMode quantifier)
    {
        return _symbols.TryGetValue(quantifier, out var symbol) ? symbol : quantifier.ToString();
    }

    public static bool TryParse(string symbol, out QuantifierMode result)
    {
        return _bySymbol.TryGetValue(symbol, out result);
    }

    public static MethodInfo GetLinqMethod(this QuantifierMode quantifier, bool withPredicate)
    {
        var parameterCount = withPredicate ? 2 : 1;

        return _linqMethods.TryGetValue((quantifier, parameterCount), out var linqMethod)
            ? linqMethod
            : throw new NotSupportedException($"LINQ method not defined for quantifier mode {quantifier}.");
    }

    private static MethodInfo ResolveLinqMethod(string methodName, int parameterCount)
    {
        return typeof(Enumerable)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Single(method => method.Name == methodName && method.GetParameters().Length == parameterCount);
    }
}
