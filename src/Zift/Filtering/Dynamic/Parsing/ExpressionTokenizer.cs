namespace Zift.Filtering.Dynamic.Parsing;

public class ExpressionTokenizer(string expression)
{
    private static readonly Dictionary<SyntaxTokenType, Regex> _regexPatternsByTokenType = new()
    {
        [SyntaxTokenType.Whitespace] = SyntaxDefinitions.Whitespace,
        [SyntaxTokenType.LogicalOperator] = SyntaxDefinitions.LogicalOperator,
        [SyntaxTokenType.UnaryLogicalOperator] = SyntaxDefinitions.UnaryLogicalOperator,
        [SyntaxTokenType.ComparisonOperator] = SyntaxDefinitions.ComparisonOperator,
        [SyntaxTokenType.Keyword] = SyntaxDefinitions.Keyword,
        [SyntaxTokenType.NumericLiteral] = SyntaxDefinitions.NumericLiteral,
        [SyntaxTokenType.StringLiteral] = SyntaxDefinitions.StringLiteral,
        [SyntaxTokenType.QuantifierMode] = SyntaxDefinitions.QuantifierMode,
        [SyntaxTokenType.CollectionProjection] = SyntaxDefinitions.CollectionProjection,
        [SyntaxTokenType.Identifier] = SyntaxDefinitions.Identifier
    };

    private static readonly Dictionary<SyntaxTokenType, string> _stringPatternsByTokenType = new()
    {
        [SyntaxTokenType.ParenthesisOpen] = SyntaxDefinitions.ParenthesisOpen,
        [SyntaxTokenType.ParenthesisClose] = SyntaxDefinitions.ParenthesisClose,
        [SyntaxTokenType.Colon] = SyntaxDefinitions.Colon,
        [SyntaxTokenType.DotSeparator] = SyntaxDefinitions.DotSeparator
    };

    private readonly string _expression = expression;
    private int _position;
    private SyntaxToken? _lookaheadToken;

    public SyntaxToken NextToken()
    {
        if (TryGetLookaheadToken(out var token))
        {
            return token;
        }

        foreach (var (type, pattern) in _regexPatternsByTokenType)
        {
            if (TryMatch(pattern, out var result, out var position))
            {
                return new(type, result, position);
            }
        }

        foreach (var (type, pattern) in _stringPatternsByTokenType)
        {
            if (TryMatch(pattern, out var result, out var position))
            {
                return new(type, result, position);
            }
        }

        return HandleUnknownOrEndToken();
    }

    public SyntaxToken PeekToken()
    {
        if (!_lookaheadToken.HasValue)
        {
            var originalPosition = _position;
            _lookaheadToken = NextToken();
            _position = originalPosition;
        }

        return _lookaheadToken.Value;
    }

    private bool TryGetLookaheadToken(out SyntaxToken result)
    {
        if (_lookaheadToken.HasValue)
        {
            result = _lookaheadToken.Value;
            _position = result.Position + result.Value.Length;
            _lookaheadToken = null;

            return true;
        }

        result = default;

        return false;
    }

    private SyntaxToken HandleUnknownOrEndToken()
    {
        var startPosition = _position;
        while (_position < _expression.Length && !IsStartOfAnyToken(_expression[_position]))
        {
            _position++;
        }

        return startPosition < _position
            ? new(SyntaxTokenType.Unknown, _expression[startPosition.._position], startPosition)
            : new(SyntaxTokenType.End, string.Empty, _position);
    }

    private bool TryMatch(Regex pattern, out string result, out int position)
    {
        var match = pattern.Match(_expression[_position..]);
        if (match.Success)
        {
            result = match.Value;
            position = _position;
            _position += match.Length;

            return true;
        }

        result = string.Empty;
        position = 0;

        return false;
    }

    private bool TryMatch(string pattern, out string result, out int position)
    {
        if (_expression[_position..].StartsWith(pattern, StringComparison.Ordinal))
        {
            result = pattern;
            position = _position;
            _position += result.Length;

            return true;
        }

        result = string.Empty;
        position = 0;

        return false;
    }

    private static bool IsStartOfAnyToken(char c)
    {
        return _stringPatternsByTokenType.Values.Any(pattern => pattern.StartsWith(c))
            || _regexPatternsByTokenType.Values.Any(pattern => pattern.IsMatch(c.ToString()));
    }
}
