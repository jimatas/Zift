namespace Zift.Querying.Parsing;

using Literals;

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
                if (NumberScanner.StartsNumber(_text.AsSpan(_position)))
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

        if (NumberScanner.StartsNumber(_text.AsSpan(_position)))
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

    private SyntaxToken ReadStringLiteral(int startPosition, char quoteChar)
    {
        _position++;

        var value = new StringBuilder();

        while (_position < _text.Length)
        {
            var currentChar = _text[_position];

            if (currentChar == quoteChar)
            {
                _position++;

                return CreateToken(SyntaxTokenType.StringLiteral, value.ToString(), startPosition);
            }

            if (currentChar == '\\')
            {
                value.Append(ReadEscapeSequence(startPosition));
                continue;
            }

            value.Append(currentChar);
            _position++;
        }

        throw new LexicalErrorException("Unterminated string literal.", startPosition);
    }

    private char ReadEscapeSequence(int literalStartPosition)
    {
        var escapePosition = _position;

        if (escapePosition + 1 >= _text.Length)
        {
            throw new LexicalErrorException("Unterminated string literal.", literalStartPosition);
        }

        var escapedChar = _text[escapePosition + 1];

        if (escapedChar is not ('\\' or '"' or '\''))
        {
            throw new LexicalErrorException(
                $"Invalid escape sequence '\\{escapedChar}'; only '\\\\', '\\\"' and '\\'' are supported.",
                escapePosition);
        }

        _position += 2;

        return escapedChar;
    }

    private bool TryReadNumber([NotNullWhen(true)] out string? number)
    {
        if (!NumberScanner.TryScan(_text.AsSpan(_position), out var parts))
        {
            number = default;

            return false;
        }

        number = _text.Substring(_position, parts.Length);
        _position += parts.Length;

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

    private SyntaxToken CreateToken(SyntaxTokenType type, int startPosition)
    {
        return CreateToken(type, _text[startPosition.._position], startPosition);
    }

    private SyntaxToken CreateToken(SyntaxTokenType type, string value, int startPosition)
    {
        return new(type, value, startPosition, _position - startPosition);
    }
}
