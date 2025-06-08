namespace Zift.Filtering.Dynamic;

/// <summary>
/// Represents a comparison operator with optional modifiers.
/// </summary>
/// <param name="Type">The base operator type (e.g., equality, greater than).</param>
public readonly record struct ComparisonOperator(ComparisonOperatorType Type)
{
    /// <summary>
    /// The modifiers applied to the operator (e.g., <c>"i"</c> for case-insensitive).
    /// </summary>
    public IReadOnlySet<string> Modifiers { get; init; } = EmptySet<string>.Instance;

    /// <summary>
    /// Determines whether the specified modifier is present.
    /// </summary>
    /// <param name="modifier">The modifier to check.</param>
    /// <returns><see langword="true"/> if the modifier is applied; otherwise, <see langword="false"/>.</returns>
    public bool HasModifier(string modifier) => Modifiers.Contains(modifier);

    /// <inheritdoc/>
    public override string ToString()
    {
        return Type + (Modifiers.Count > 0 ? ":" + string.Join(":", Modifiers) : string.Empty);
    }
}
