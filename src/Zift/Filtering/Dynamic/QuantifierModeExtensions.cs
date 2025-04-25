namespace Zift.Filtering.Dynamic;

public static class QuantifierModeExtensions
{
    private static readonly ConcurrentDictionary<(string, int), MethodInfo> _linqMethodCache = new();

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
            .Single(method
                => method.Name == signature.MethodName
                && method.GetParameters().Length == signature.ParameterCount);
    }
}
