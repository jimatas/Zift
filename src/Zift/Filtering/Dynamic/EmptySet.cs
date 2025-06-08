namespace Zift.Filtering.Dynamic;

internal static class EmptySet<T>
{
    /// <summary>
    /// A shared empty read-only set instance for the specified type <typeparamref name="T"/>.
    /// </summary>
    public static readonly IReadOnlySet<T> Instance = new HashSet<T>();
}
