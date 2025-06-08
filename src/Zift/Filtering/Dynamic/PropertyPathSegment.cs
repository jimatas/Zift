namespace Zift.Filtering.Dynamic;

/// <summary>
/// Represents a single segment in a property path, optionally modified with a quantifier or projection.
/// </summary>
/// <param name="Name">The name of the property represented by this segment.</param>
public readonly record struct PropertyPathSegment(string Name)
{
    /// <summary>
    /// The quantifier modifier applied to this segment, if any (e.g., <c>:any</c>, <c>:all</c>).
    /// </summary>
    public QuantifierMode? Quantifier { get; init; }

    /// <summary>
    /// The collection projection applied to this segment, if any (e.g., <c>:count</c>).
    /// </summary>
    public CollectionProjection? Projection { get; init; }

    /// <summary>
    /// Indicates whether this segment is terminal due to a projection (e.g., <c>:count</c>).
    /// </summary>
    public bool IsTerminal => Projection.HasValue && Projection.Value.IsTerminal();

    /// <inheritdoc/>
    public override string ToString() => ToString(includeModifier: false);

    /// <summary>
    /// Returns the string representation of the segment, optionally including the modifier.
    /// </summary>
    /// <param name="includeModifier">Whether to include the quantifier or projection modifier.</param>
    /// <returns>The formatted segment string.</returns>
    public string ToString(bool includeModifier)
    {
        if (includeModifier && Projection.HasValue)
        {
            return $"{Name}:{Projection.Value.ToSymbol()}";
        }

        if (includeModifier && Quantifier.HasValue)
        {
            return $"{Name}:{Quantifier.Value.ToSymbol()}";
        }

        return Name;
    }

    internal void Validate(bool isLastSegment)
    {
        if (Quantifier.HasValue && Projection.HasValue)
        {
            throw new InvalidOperationException("A property segment cannot have more than one modifier (e.g., both ':any' and ':count').");
        }

        if (IsTerminal && !isLastSegment)
        {
            throw new InvalidOperationException("Terminal collection projections (e.g., ':count') must appear at the end of the property path.");
        }
    }
}
