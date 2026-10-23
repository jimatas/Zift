namespace Zift.Querying.Model;

internal sealed record QuantifierNode(
    PropertyPathNode Source,
    QuantifierKind Kind,
    PredicateNode? Predicate) : PredicateNode;
