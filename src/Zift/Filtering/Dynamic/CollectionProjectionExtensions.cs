namespace Zift.Filtering.Dynamic;

public static class CollectionProjectionExtensions
{
    private static readonly ConcurrentDictionary<string, MethodInfo> _linqMethodCache = new();

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
            .Single(method => method.Name == name && method.GetParameters().Length == 1);
    }
}
    