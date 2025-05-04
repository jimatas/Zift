namespace Zift.Filtering.Dynamic;

public static class QuantifierModeExtensions
{
    private static readonly ConcurrentDictionary<(string, int), MethodInfo> _linqMethodCache = new();
    private static readonly Dictionary<QuantifierMode, string> _displayNames = new()
    {
        [QuantifierMode.Any] = "any",
        [QuantifierMode.All] = "all"
    };

    public static string ToDisplayString(this QuantifierMode quantifier)
    {
        return _displayNames.TryGetValue(quantifier, out var displayName) ? displayName : quantifier.ToString();
    }

    public static bool TryParse(string value, out QuantifierMode result)
    {
        foreach (var (candidate, displayName) in _displayNames)
        {
            if (displayName.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                result = candidate;
                return true;
            }
        }

        result = default;
        return false;
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
