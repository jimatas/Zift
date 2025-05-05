namespace Zift.Filtering.Dynamic.Parsing;

public static class SyntaxDefinitions
{
    public static readonly Regex Whitespace = new(@"^\s+", RegexOptions.Compiled);
    public static readonly Regex LogicalOperator = new(@"^(?:&&|\|\|)", RegexOptions.Compiled);
    public static readonly Regex UnaryLogicalOperator = new(@"^!(?!=)", RegexOptions.Compiled);
    public static readonly Regex ComparisonOperator = new(@"^(?:==|!=|<=|<|>=|>|%=|\^=|\$=)", RegexOptions.Compiled);
    public static readonly Regex Keyword = new(@"^(?:true|false|null)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex NumericLiteral = new(@"^[+-]?[0-9]+(?:\.[0-9]+)?(?:[eE][+-]?[0-9]+)?", RegexOptions.Compiled);
    public static readonly Regex StringLiteral = new(@"^(?:""(?:\\.|[^""\\])*""|'(?:\\.|[^'\\])*')", RegexOptions.Compiled);
    public static readonly Regex Identifier = new(@"^[a-zA-Z_][a-zA-Z0-9_]*", RegexOptions.Compiled);
    public const string ParenthesisOpen = "(";
    public const string ParenthesisClose = ")";
    public const string BracketOpen = "[";
    public const string BracketClose = "]";
    public const string Colon = ":";
    public const string DotSeparator = ".";
    public const string Comma = ",";
}
