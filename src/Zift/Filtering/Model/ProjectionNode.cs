namespace Zift.Filtering.Model;

internal sealed record ProjectionNode(
    PropertyPathNode Source,
    CollectionProjection Projection) : PropertyNode;
