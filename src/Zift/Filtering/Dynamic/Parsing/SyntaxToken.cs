namespace Zift.Filtering.Dynamic.Parsing;

public readonly record struct SyntaxToken(SyntaxTokenType Type, string Value, int Position);
