namespace Zift.Filtering.Parsing;

internal readonly record struct SyntaxToken(
    SyntaxTokenType Type,
    string Text,
    int Position,
    int Length);
