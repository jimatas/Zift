namespace Zift.Filtering.Dynamic;

internal static class CollectionUtilities
{
    public static IReadOnlySet<T> EmptySet<T>() => EmptyHashSet<T>.Instance;

    private static class EmptyHashSet<T>
    {
        public static readonly IReadOnlySet<T> Instance = new HashSet<T>();
    }
}
