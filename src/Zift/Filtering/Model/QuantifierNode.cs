namespace Zift.Filtering.Model;

internal sealed record QuantifierNode(
    PropertyPathNode Source,
    QuantifierKind Kind,
    PredicateNode? Predicate) : PredicateNode;
