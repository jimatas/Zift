namespace Zift.Querying.Model;

internal sealed record QuantifierNode(
    PropertyNode Source,
    QuantifierKind Kind,
    PredicateNode? Predicate) : PredicateNode;
