namespace Zift.Querying.Model;

internal sealed record ListLiteral(
    IReadOnlyList<LiteralNode> Items) : LiteralNode;
