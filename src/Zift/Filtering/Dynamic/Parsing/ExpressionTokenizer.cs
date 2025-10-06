namespace Zift.Filtering.Dynamic.Parsing;

/// <summary>
/// Tokenizes a dynamic filter expression into a sequence of <see cref="SyntaxToken"/> values.
/// </summary>
/// <param name="expression">The filter expression to tokenize.</param>
public sealed class ExpressionTokenizer(string expression)
{
    private readonly string _expression = expression;
    private int _position;
    private SyntaxToken? _lookaheadToken;

    /// <summary>
    /// Retrieves the next token in the expression.
    /// </summary>
    public SyntaxToken NextToken()
    {
        if (TryGetLookaheadToken(out var token))
        {
            return token;
        }

        foreach (var rule in SyntaxRules.All)
        {
            if (rule.Pattern is { } pattern && TryMatch(pattern, out var result, out var position))
            {
                return new(rule.Type, result, position);
            }

            if (rule.Symbol is { } symbol && TryMatch(symbol, out result, out position))
            {
                return new(rule.Type, result, position);
            }
        }

        return HandleUnknownOrEndToken();
    }

    /// <summary>
    /// Returns the next token without advancing the tokenizer.
    /// </summary>
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

    private bool TryMatch(string symbol, out string result, out int position)
    {
        if (_expression[_position..].StartsWith(symbol, StringComparison.Ordinal))
        {
            result = symbol;
            position = _position;
            _position += result.Length;

            return true;
        }

        result = string.Empty;
        position = 0;

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

    private static bool IsStartOfAnyToken(char c)
    {
        return SyntaxRules.All.Any(rule =>
            rule.Symbol is { } symbol && symbol.StartsWith(c) ||
            rule.Pattern is { } pattern && pattern.IsMatch(c.ToString()));
    }
}
