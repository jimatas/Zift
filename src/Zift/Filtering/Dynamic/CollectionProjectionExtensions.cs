namespace Zift.Filtering.Dynamic;

/// <summary>
/// Extension methods for working with <see cref="CollectionProjection"/> values, such as symbol conversion
/// and LINQ method resolution.
/// </summary>
public static class CollectionProjectionExtensions
{
    private static readonly Dictionary<CollectionProjection, string> _symbols = new()
    {
        [CollectionProjection.Count] = "count"
    };

    private static readonly Dictionary<string, CollectionProjection> _bySymbol =
        _symbols.ToDictionary(
            kvp => kvp.Value,
            kvp => kvp.Key,
            StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<CollectionProjection, MethodInfo> _linqMethods = new()
    {
        [CollectionProjection.Count] = ResolveLinqMethod(nameof(Enumerable.Count))
    };

    /// <summary>
    /// Returns the symbolic representation (e.g., <c>"count"</c>) for the given collection projection.
    /// </summary>
    /// <param name="projection">The collection projection.</param>
    /// <returns>The symbolic representation of the projection.</returns>
    public static string ToSymbol(this CollectionProjection projection) =>
        _symbols.TryGetValue(projection, out var symbol)
            ? symbol
            : projection.ToString();

    /// <summary>
    /// Tries to parse a symbol into a <see cref="CollectionProjection"/>.
    /// </summary>
    /// <param name="symbol">The symbol to parse (e.g., <c>"count"</c>).</param>
    /// <param name="result">When this method returns, contains the parsed projection if successful.</param>
    /// <returns><see langword="true"/> if the symbol was recognized; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string symbol, out CollectionProjection result) =>
        _bySymbol.TryGetValue(symbol, out result);

    /// <summary>
    /// Determines whether the projection is terminal (i.e., produces a scalar value).
    /// </summary>
    /// <param name="projection">The projection to evaluate.</param>
    /// <returns><see langword="true"/> if the projection is terminal; otherwise, <see langword="false"/>.</returns>
    public static bool IsTerminal(this CollectionProjection projection) =>
        projection == CollectionProjection.Count;

    /// <summary>
    /// Gets the corresponding LINQ method for the given collection projection.
    /// </summary>
    /// <param name="projection">The projection to resolve.</param>
    /// <returns>The <see cref="MethodInfo"/> representing the LINQ method.</returns>
    /// <exception cref="NotSupportedException">Thrown if no LINQ method is defined for the given projection.</exception>
    public static MethodInfo GetLinqMethod(this CollectionProjection projection) =>
        _linqMethods.TryGetValue(projection, out var linqMethod)
            ? linqMethod
            : throw new NotSupportedException($"LINQ method not defined for collection projection {projection}.");

    private static MethodInfo ResolveLinqMethod(string methodName) =>
        typeof(Enumerable)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Single(method =>
                method.Name == methodName &&
                method.GetParameters().Length == 1);
}
