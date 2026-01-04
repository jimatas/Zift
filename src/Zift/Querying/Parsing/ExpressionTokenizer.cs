namespace Zift.Querying.Parsing;

internal sealed class ExpressionTokenizer(string text)
{
    private readonly string _text = text;
    private int _position;
    private SyntaxToken? _lookaheadToken;

    public SyntaxToken PeekToken()
    {
        if (_lookaheadToken is null)
        {
            var originalPosition = _position;

            _lookaheadToken = ReadToken();
            _position = originalPosition;
        }

        return _lookaheadToken.Value;
    }

    public SyntaxToken NextToken()
    {
        if (_lookaheadToken is { } cachedToken)
        {
            _lookaheadToken = null;
            _position = cachedToken.Position + cachedToken.Length;

            return cachedToken;
        }

        return ReadToken();
    }

    private SyntaxToken ReadToken()
    {
        SkipWhitespace();

        if (_position >= _text.Length)
        {
            return new(SyntaxTokenType.End, string.Empty, _text.Length, 0);
        }

        var startPosition = _position;
        var currentChar = _text[_position];

        switch (currentChar)
        {
            case '.':
                if (NextCharIs(char.IsAsciiDigit))
                {
                    break;
                }

                _position++;
                return CreateToken(SyntaxTokenType.Dot, startPosition);

            case ':':
                _position++;
                return CreateToken(SyntaxTokenType.Colon, startPosition);

            case ',':
                _position++;
                return CreateToken(SyntaxTokenType.Comma, startPosition);

            case '(':
                _position++;
                return CreateToken(SyntaxTokenType.ParenOpen, startPosition);

            case ')':
                _position++;
                return CreateToken(SyntaxTokenType.ParenClose, startPosition);

            case '[':
                _position++;
                return CreateToken(SyntaxTokenType.BracketOpen, startPosition);

            case ']':
                _position++;
                return CreateToken(SyntaxTokenType.BracketClose, startPosition);

            case '&':
                if (NextCharIs('&'))
                {
                    _position += 2;
                    return CreateToken(SyntaxTokenType.LogicalAnd, startPosition);
                }

                throw new LexicalErrorException(
                    "Unexpected character '&'; did you mean '&&'?",
                    startPosition);

            case '|':
                if (NextCharIs('|'))
                {
                    _position += 2;
                    return CreateToken(SyntaxTokenType.LogicalOr, startPosition);
                }

                throw new LexicalErrorException(
                    "Unexpected character '|'; did you mean '||'?",
                    startPosition);

            case '!':
                if (NextCharIs('='))
                {
                    _position += 2;
                    return CreateToken(SyntaxTokenType.NotEqual, startPosition);
                }

                _position++;
                return CreateToken(SyntaxTokenType.LogicalNot, startPosition);

            case '=':
                if (NextCharIs('='))
                {
                    _position += 2;
                    return CreateToken(SyntaxTokenType.Equal, startPosition);
                }

                throw new LexicalErrorException(
                    "Unexpected character '='; did you mean '=='?",
                    startPosition);

            case '<':
                if (NextCharIs('='))
                {
                    _position += 2;
                    return CreateToken(SyntaxTokenType.LessThanOrEqual, startPosition);
                }

                _position++;
                return CreateToken(SyntaxTokenType.LessThan, startPosition);

            case '>':
                if (NextCharIs('='))
                {
                    _position += 2;
                    return CreateToken(SyntaxTokenType.GreaterThanOrEqual, startPosition);
                }

                _position++;
                return CreateToken(SyntaxTokenType.GreaterThan, startPosition);

            case '%':
                if (NextCharIs('='))
                {
                    _position += 2;
                    return CreateToken(SyntaxTokenType.Contains, startPosition);
                }

                throw new LexicalErrorException(
                    "Unexpected character '%'; did you mean '%='?",
                    startPosition);

            case '^':
                if (NextCharIs('='))
                {
                    _position += 2;
                    return CreateToken(SyntaxTokenType.StartsWith, startPosition);
                }

                throw new LexicalErrorException(
                    "Unexpected character '^'; did you mean '^='?",
                    startPosition);

            case '$':
                if (NextCharIs('='))
                {
                    _position += 2;
                    return CreateToken(SyntaxTokenType.EndsWith, startPosition);
                }

                throw new LexicalErrorException(
                    "Unexpected character '$'; did you mean '$='?",
                    startPosition);
        }

        if (currentChar == '"' || currentChar == '\'')
        {
            return ReadStringLiteral(startPosition, currentChar);
        }

        if (IsNumberStart())
        {
            if (TryReadNumber(out var number))
            {
                return CreateToken(
                    SyntaxTokenType.NumberLiteral,
                    number,
                    startPosition);
            }

            throw new LexicalErrorException("Invalid numeric literal.", startPosition);
        }

        if (char.IsAsciiLetter(currentChar) || currentChar == '_')
        {
            return ReadIdentifier(startPosition);
        }

        throw new LexicalErrorException($"Unexpected character '{currentChar}'.", startPosition);
    }

    private bool NextCharIs(char expectedChar)
    {
        return _position + 1 < _text.Length && _text[_position + 1] == expectedChar;
    }

    private bool NextCharIs(Func<char, bool> predicate)
    {
        return _position + 1 < _text.Length && predicate(_text[_position + 1]);
    }

    private SyntaxToken ReadStringLiteral(int startPosition, char quoteChar)
    {
        _position++;

        while (_position < _text.Length)
        {
            if (_text[_position] == quoteChar && IsUnescapedQuote(startPosition))
            {
                break;
            }

            _position++;
        }

        if (_position >= _text.Length)
        {
            throw new LexicalErrorException("Unterminated string literal.", startPosition);
        }

        _position++;

        var value = _text[(startPosition + 1)..(_position - 1)];

        return CreateToken(SyntaxTokenType.StringLiteral, value, startPosition);
    }

    private bool IsUnescapedQuote(int openingQuotePosition)
    {
        var backslashCount = 0;
        var i = _position - 1;

        while (i > openingQuotePosition && _text[i] == '\\')
        {
            backslashCount++;
            i--;
        }

        return (backslashCount & 1) == 0;
    }

    private bool IsNumberStart()
    {
        var c = _text[_position];

        if (char.IsAsciiDigit(c))
        {
            return true;
        }

        if (c == '.' && _position + 1 < _text.Length &&
            char.IsAsciiDigit(_text[_position + 1]))
        {
            return true;
        }

        if ((c == '+' || c == '-') && _position + 1 < _text.Length)
        {
            var nextChar = _text[_position + 1];
            if (char.IsAsciiDigit(nextChar))
            {
                return true;
            }

            if (nextChar == '.' && _position + 2 < _text.Length)
            {
                nextChar = _text[_position + 2];
                return char.IsAsciiDigit(nextChar);
            }
        }

        return false;
    }

    private bool TryReadNumber([NotNullWhen(true)] out string? number)
    {
        number = default;

        var startPosition = _position;
        var i = _position;

        var hasDigit = false;

        var hasFraction = false;
        var hasFractionDigit = false;

        var hasExponent = false;
        var hasExponentDigit = false;

        if (_text[i] == '+' || _text[i] == '-')
        {
            i++;
        }

        while (i < _text.Length)
        {
            var c = _text[i];

            if (char.IsAsciiDigit(c))
            {
                hasDigit = true;

                if (hasExponent)
                {
                    hasExponentDigit = true;
                }
                else if (hasFraction)
                {
                    hasFractionDigit = true;
                }

                i++;
                continue;
            }

            if (c == '.' && !hasFraction && !hasExponent)
            {
                hasFraction = true;
                hasFractionDigit = false;
                i++;
                continue;
            }

            if ((c == 'e' || c == 'E') && hasDigit && !hasExponent)
            {
                if (hasFraction && !hasFractionDigit)
                {
                    return false;
                }

                hasExponent = true;
                hasExponentDigit = false;
                i++;

                if (i < _text.Length && (_text[i] == '+' || _text[i] == '-'))
                {
                    i++;
                }

                continue;
            }

            break;
        }

        if (!hasDigit || (hasFraction && !hasFractionDigit) || (hasExponent && !hasExponentDigit))
        {
            return false;
        }

        number = _text[startPosition..i];
        _position = i;

        return true;
    }

    private SyntaxToken ReadIdentifier(int startPosition)
    {
        _position++;

        while (_position < _text.Length &&
            (char.IsAsciiLetterOrDigit(_text[_position]) || _text[_position] == '_'))
        {
            _position++;
        }

        var identifier = _text[startPosition.._position];

        return identifier switch
        {
            "true" => CreateToken(SyntaxTokenType.True, identifier, startPosition),
            "false" => CreateToken(SyntaxTokenType.False, identifier, startPosition),
            "null" => CreateToken(SyntaxTokenType.Null, identifier, startPosition),
            "in" => CreateToken(SyntaxTokenType.In, identifier, startPosition),
            "any" => CreateToken(SyntaxTokenType.Any, identifier, startPosition),
            "all" => CreateToken(SyntaxTokenType.All, identifier, startPosition),
            _ => CreateToken(SyntaxTokenType.Identifier, identifier, startPosition)
        };
    }

    private void SkipWhitespace()
    {
        while (_position < _text.Length &&
            char.IsWhiteSpace(_text[_position]))
        {
            _position++;
        }
    }

    private SyntaxToken CreateToken(SyntaxTokenType type, int startPosition) =>
        CreateToken(type, _text[startPosition.._position], startPosition);

    private SyntaxToken CreateToken(SyntaxTokenType type, string value, int startPosition) =>
        new(type, value, startPosition, _position - startPosition);
}
