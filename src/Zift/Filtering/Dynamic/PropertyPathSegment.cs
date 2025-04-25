namespace Zift.Filtering.Dynamic;

public readonly record struct PropertyPathSegment(string Name)
{
    public QuantifierMode? Quantifier { get; init; }
    public CollectionProjection? Projection { get; init; }
    public bool IsTerminal => Projection.HasValue && Projection.Value.IsTerminal();

    public override string ToString() => ToString(includeModifier: false);

    public string ToString(bool includeModifier)
    {
        if (includeModifier && Projection.HasValue)
        {
            return $"{Name}:{Projection.Value.ToString().ToLowerInvariant()}";
        }

        if (includeModifier && Quantifier.HasValue)
        {
            return $"{Name}:{Quantifier.Value.ToString().ToLowerInvariant()}";
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
