namespace Zift.Filtering.Dynamic.Parsing;

public enum SyntaxTokenType
{
    Unknown = 0,
    Whitespace,
    ParenthesisOpen,
    ParenthesisClose,
    BracketOpen,
    BracketClose,
    LogicalOperator,
    UnaryLogicalOperator,
    ComparisonOperator,
    Keyword,
    NumericLiteral,
    StringLiteral,
    Identifier,
    Colon,
    DotSeparator,
    Comma,
    End
}
