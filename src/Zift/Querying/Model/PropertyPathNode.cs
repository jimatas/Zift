namespace Zift.Querying.Model;

internal sealed record PropertyPathNode(
    IReadOnlyList<string> Segments) : PropertyNode;
