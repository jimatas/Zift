namespace Zift.Querying.Model;

internal sealed record ComparisonNode(
    PropertyNode Left,
    ComparisonOperator Operator,
    LiteralNode Right) : PredicateNode;
