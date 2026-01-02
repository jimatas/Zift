namespace Zift.Querying.Parsing;

internal enum SyntaxTokenType
{
    Identifier,
    NumberLiteral,
    StringLiteral,
    True,
    False,
    Null,
    Any,
    All,
    LogicalAnd,
    LogicalOr,
    LogicalNot,
    Equal,
    NotEqual,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,
    Contains,
    StartsWith,
    EndsWith,
    In,
    Dot,
    Colon,
    Comma,
    ParenOpen,
    ParenClose,
    BracketOpen,
    BracketClose,
    End
}
