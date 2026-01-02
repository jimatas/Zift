namespace Zift.Querying.Model;

internal sealed record ProjectionNode(
    PropertyNode Source,
    CollectionProjection Projection) : PropertyNode;
