namespace Zift.Filtering.Dynamic;

public static class CollectionProjectionExtensions
{
    private static readonly ConcurrentDictionary<string, MethodInfo> _linqMethodCache = new();
    private static readonly Dictionary<CollectionProjection, string> _symbolMap = new()
    {
        [CollectionProjection.Count] = "count"
    };

    public static string ToSymbol(this CollectionProjection projection)
    {
        return _symbolMap.TryGetValue(projection, out var symbol) ? symbol : projection.ToString();
    }

    public static CollectionProjection FromSymbol(string symbol)
    {
        foreach (var (projection, mappedSymbol) in _symbolMap)
        {
            if (mappedSymbol.Equals(symbol, StringComparison.OrdinalIgnoreCase))
            {
                return projection;
            }
        }

        throw new ArgumentException($"Unknown collection projection: {symbol}", nameof(symbol));
    }

    public static bool IsTerminal(this CollectionProjection projection)
    {
        return projection == CollectionProjection.Count;
    }

    public static MethodInfo ToLinqMethod(this CollectionProjection projection)
    {
        return _linqMethodCache.GetOrAdd(projection.ToString(), ResolveLinqMethod);
    }

    private static MethodInfo ResolveLinqMethod(string name)
    {
        return typeof(Enumerable)
            .GetMethods()
            .Single(method =>
                method.Name == name
                && method.GetParameters().Length == 1);
    }
}
    