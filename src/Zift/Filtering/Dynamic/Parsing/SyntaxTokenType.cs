namespace Zift.Filtering.Dynamic.Parsing;

/// <summary>
/// Defines the types of syntax tokens recognized in dynamic filter expressions.
/// </summary>
public enum SyntaxTokenType
{
    /// <summary>
    /// Unrecognized token.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Whitespace characters.
    /// </summary>
    Whitespace,

    /// <summary>
    /// Left parenthesis token: <c>(</c>.
    /// </summary>
    ParenthesisOpen,

    /// <summary>
    /// Right parenthesis token: <c>)</c>.
    /// </summary>
    ParenthesisClose,

    /// <summary>
    /// Left bracket token: <c>[</c>.
    /// </summary>
    BracketOpen,

    /// <summary>
    /// Right bracket token: <c>]</c>.
    /// </summary>
    BracketClose,

    /// <summary>
    /// Binary logical operator (e.g., <c>&amp;&amp;</c>, <c>||</c>).
    /// </summary>
    LogicalOperator,

    /// <summary>
    /// Unary logical operator (e.g., <c>!</c>).
    /// </summary>
    UnaryLogicalOperator,

    /// <summary>
    /// Comparison operator (e.g., <c>==</c>, <c>in</c>, <c>%=</c>).
    /// </summary>
    ComparisonOperator,

    /// <summary>
    /// Literal keyword (e.g., <c>true</c>, <c>false</c>, <c>null</c>).
    /// </summary>
    Keyword,

    /// <summary>
    /// Numeric literal.
    /// </summary>
    NumericLiteral,

    /// <summary>
    /// Quoted string literal.
    /// </summary>
    StringLiteral,

    /// <summary>
    /// Identifier (e.g., property name).
    /// </summary>
    Identifier,

    /// <summary>
    /// Colon token: <c>:</c>.
    /// </summary>
    Colon,

    /// <summary>
    /// Dot separator token: <c>.</c>.
    /// </summary>
    DotSeparator,

    /// <summary>
    /// Comma separator token: <c>,</c>.
    /// </summary>
    Comma,

    /// <summary>
    /// End of input.
    /// </summary>
    End
}
