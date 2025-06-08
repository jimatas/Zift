namespace Zift.Filtering.Dynamic;

/// <summary>
/// Extension methods for working with <see cref="QuantifierMode"/> values, such as symbol parsing and LINQ method resolution.
/// </summary>
public static class QuantifierModeExtensions
{
    private static readonly Dictionary<QuantifierMode, string> _symbols = new()
    {
        [QuantifierMode.Any] = "any",
        [QuantifierMode.All] = "all"
    };

    private static readonly Dictionary<string, QuantifierMode> _bySymbol = _symbols.ToDictionary(
        pair => pair.Value,
        pair => pair.Key,
        StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<(QuantifierMode, int), MethodInfo> _linqMethods = new()
    {
        [(QuantifierMode.Any, 1)] = ResolveLinqMethod(nameof(Enumerable.Any), 1),
        [(QuantifierMode.Any, 2)] = ResolveLinqMethod(nameof(Enumerable.Any), 2),
        [(QuantifierMode.All, 2)] = ResolveLinqMethod(nameof(Enumerable.All), 2)
    };

    /// <summary>
    /// Returns the symbolic representation of the quantifier (e.g., <c>"any"</c>, <c>"all"</c>).
    /// </summary>
    /// <param name="quantifier">The quantifier mode.</param>
    /// <returns>The corresponding symbol string.</returns>
    public static string ToSymbol(this QuantifierMode quantifier)
    {
        return _symbols.TryGetValue(quantifier, out var symbol) ? symbol : quantifier.ToString();
    }

    /// <summary>
    /// Attempts to parse a symbol into a <see cref="QuantifierMode"/>.
    /// </summary>
    /// <param name="symbol">The symbol to parse (e.g., <c>"any"</c>).</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="QuantifierMode"/>, if successful.</param>
    /// <returns><see langword="true"/> if parsing succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string symbol, out QuantifierMode result)
    {
        return _bySymbol.TryGetValue(symbol, out result);
    }

    /// <summary>
    /// Gets the corresponding LINQ method for the given quantifier and predicate usage.
    /// </summary>
    /// <param name="quantifier">The quantifier mode.</param>
    /// <param name="withPredicate"><see langword="true"/> to retrieve the overload that includes a predicate.</param>
    /// <returns>The matching <see cref="MethodInfo"/> for <c>Enumerable.Any</c> or <c>Enumerable.All</c>.</returns>
    /// <exception cref="NotSupportedException">Thrown if no matching method is defined for the quantifier.</exception>
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
