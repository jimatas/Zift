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
    QuantifierMode,
    CollectionProjection,
    Identifier,
    Colon,
    DotSeparator,
    End
}
