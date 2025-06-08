namespace Zift.Filtering.Dynamic.Parsing;

/// <summary>
/// Provides a collection of predefined syntax rules for tokenizing filter expressions.
/// </summary>
public static class SyntaxRules
{
    /// <summary>
    /// All supported syntax rules in evaluation order, used to tokenize dynamic filter expressions.
    /// </summary>
    public static readonly IReadOnlyList<SyntaxRule> All =
    [
        new(SyntaxTokenType.Whitespace) { Pattern = new(@"^\s+", RegexOptions.Compiled) },
        new(SyntaxTokenType.LogicalOperator) { Pattern = new(@"^(?:&&|\|\|)", RegexOptions.Compiled) },
        new(SyntaxTokenType.UnaryLogicalOperator) { Pattern = new(@"^!(?!=)", RegexOptions.Compiled) },
        new(SyntaxTokenType.ComparisonOperator) { Pattern = new(@"^(?:==|!=|<=|<|>=|>|%=|\^=|\$=|in\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled) },
        new(SyntaxTokenType.Keyword) { Pattern = new(@"^(?:true|false|null)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled) },
        new(SyntaxTokenType.NumericLiteral) { Pattern = new(@"^[+-]?[0-9]+(?:\.[0-9]+)?(?:[eE][+-]?[0-9]+)?", RegexOptions.Compiled) },
        new(SyntaxTokenType.StringLiteral) { Pattern = new(@"^(?:""(?:\\.|[^""\\])*""|'(?:\\.|[^'\\])*')", RegexOptions.Compiled) },
        new(SyntaxTokenType.Identifier) { Pattern = new(@"^[a-zA-Z_][a-zA-Z0-9_]*", RegexOptions.Compiled) },
        new(SyntaxTokenType.ParenthesisOpen) { Symbol = "(" },
        new(SyntaxTokenType.ParenthesisClose) { Symbol = ")" },
        new(SyntaxTokenType.BracketOpen) { Symbol = "[" },
        new(SyntaxTokenType.BracketClose) { Symbol = "]" },
        new(SyntaxTokenType.Colon) { Symbol = ":" },
        new(SyntaxTokenType.DotSeparator) { Symbol = "." },
        new(SyntaxTokenType.Comma) { Symbol = "," }
    ];
}
