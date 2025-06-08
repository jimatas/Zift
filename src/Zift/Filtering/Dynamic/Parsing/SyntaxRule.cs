namespace Zift.Filtering.Dynamic.Parsing;

/// <summary>
/// Represents a lexical syntax rule used to match tokens in a filter expression.
/// </summary>
/// <param name="Type">The type of syntax token this rule applies to.</param>
public readonly record struct SyntaxRule(SyntaxTokenType Type)
{
    /// <summary>
    /// The regular expression pattern used to match tokens of the given type.
    /// </summary>
    public Regex? Pattern { get; init; }

    /// <summary>
    /// The literal symbol used to match fixed tokens (e.g., punctuation or keywords).
    /// </summary>
    public string? Symbol { get; init; }
}
