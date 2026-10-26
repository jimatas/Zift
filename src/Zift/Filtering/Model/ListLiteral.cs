namespace Zift.Filtering.Model;

internal sealed record ListLiteral(
    IReadOnlyList<LiteralNode> Items) : LiteralNode;
