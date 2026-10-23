namespace Zift.Querying.Model;

internal sealed record ProjectionNode(
    PropertyPathNode Source,
    CollectionProjection Projection) : PropertyNode;
