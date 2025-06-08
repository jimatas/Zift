namespace Zift.Filtering.Dynamic.Parsing;

/// <summary>
/// Represents a single token extracted from a filter expression.
/// </summary>
/// <param name="Type">The type of the syntax token.</param>
/// <param name="Value">The raw value of the token.</param>
/// <param name="Position">The position of the token in the original expression.</param>
public readonly record struct SyntaxToken(SyntaxTokenType Type, string Value, int Position);
