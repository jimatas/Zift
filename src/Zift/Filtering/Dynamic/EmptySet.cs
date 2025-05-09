namespace Zift.Filtering.Dynamic;

internal static class EmptySet<T>
{
    public static readonly IReadOnlySet<T> Instance = new HashSet<T>();
}
