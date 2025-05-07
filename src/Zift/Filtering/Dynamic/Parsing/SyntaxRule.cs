namespace Zift.Filtering.Dynamic.Parsing;

public readonly record struct SyntaxRule(SyntaxTokenType Type)
{
    public Regex? Pattern { get; init; }
    public string? Symbol { get; init; }
}
