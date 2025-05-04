namespace Zift.Filtering.Dynamic.Parsing;

public enum SyntaxTokenType
{
    Unknown = 0,
    Whitespace,
    ParenthesisOpen,
    ParenthesisClose,
    LogicalOperator,
    UnaryLogicalOperator,
    ComparisonOperator,
    Keyword,
    NumericLiteral,
    StringLiteral,
    Identifier,
    Colon,
    DotSeparator,
    End
}
