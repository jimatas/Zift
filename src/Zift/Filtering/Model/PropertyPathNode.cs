namespace Zift.Filtering.Model;

internal sealed record PropertyPathNode(
    IReadOnlyList<string> Segments) : PropertyNode;
