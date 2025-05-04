namespace Zift.Filtering.Dynamic;

public static class CollectionProjectionExtensions
{
    private static readonly ConcurrentDictionary<string, MethodInfo> _linqMethodCache = new();
    private static readonly Dictionary<CollectionProjection, string> _displayNames = new()
    {
        [CollectionProjection.Count] = "count"
    };

    public static string ToDisplayString(this CollectionProjection projection)
    {
        return _displayNames.TryGetValue(projection, out var displayName) ? displayName : projection.ToString();
    }

    public static bool TryParse(string value, out CollectionProjection result)
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
