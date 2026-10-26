namespace Zift.Filtering.Model;

internal sealed record ComparisonNode(
    PropertyNode Left,
    ComparisonOperator Operator,
    LiteralNode Right) : PredicateNode;
