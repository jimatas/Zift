namespace Zift.Querying.Model;

internal sealed record LogicalNode(
    LogicalOperator Operator,
    IReadOnlyList<PredicateNode> Terms) : PredicateNode;
