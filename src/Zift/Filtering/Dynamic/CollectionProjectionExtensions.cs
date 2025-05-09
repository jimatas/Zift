namespace Zift.Filtering.Dynamic;

public static class CollectionProjectionExtensions
{
    private static readonly Dictionary<CollectionProjection, string> _symbols = new()
    {
        [CollectionProjection.Count] = "count"
    };

    private static readonly Dictionary<string, CollectionProjection> _bySymbol = _symbols.ToDictionary(
        pair => pair.Value,
        pair => pair.Key,
        StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<CollectionProjection, MethodInfo> _linqMethods = new()
    {
        [CollectionProjection.Count] = ResolveLinqMethod(nameof(Enumerable.Count))
    };

    public static string ToSymbol(this CollectionProjection projection)
    {
        return _symbols.TryGetValue(projection, out var symbol) ? symbol : projection.ToString();
    }

    public static bool TryParse(string symbol, out CollectionProjection result)
    {
        return _bySymbol.TryGetValue(symbol, out result);
    }

    public static bool IsTerminal(this CollectionProjection projection)
    {
        return projection == CollectionProjection.Count;
    }

    public static MethodInfo GetLinqMethod(this CollectionProjection projection)
    {
        return _linqMethods.TryGetValue(projection, out var linqMethod)
            ? linqMethod
            : throw new NotSupportedException($"LINQ method not defined for collection projection {projection}.");
    }

    private static MethodInfo ResolveLinqMethod(string methodName)
    {
        return typeof(Enumerable)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Single(method => method.Name == methodName && method.GetParameters().Length == 1);
    }
}
