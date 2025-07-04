﻿namespace Zift.Filtering.Dynamic.Parsing;

/// <summary>
/// Parses a comparison operator and any associated modifiers from the token stream.
/// </summary>
/// <param name="tokenizer">The tokenizer providing the input tokens.</param>
internal class ComparisonOperatorParser(ExpressionTokenizer tokenizer)
{
    private readonly ExpressionTokenizer _tokenizer = tokenizer;

    /// <summary>
    /// Parses a comparison operator with optional modifiers.
    /// </summary>
    /// <returns>The parsed <see cref="ComparisonOperator"/>.</returns>
    /// <exception cref="SyntaxErrorException">Thrown if the operator or modifiers are invalid.</exception>
    public ComparisonOperator Parse()
    {
        var token = _tokenizer.NextNonWhitespaceToken();
        if (token.Type != SyntaxTokenType.ComparisonOperator)
        {
            throw new SyntaxErrorException($"Expected a comparison operator, but got: {token.Value}", token);
        }

        var symbol = token.Value;
        var modifiers = ParseModifiers();

        _ = ComparisonOperatorType.TryParse(symbol, out var @operator);

        var unsupported = modifiers.Except(@operator.SupportedModifiers, StringComparer.OrdinalIgnoreCase);
        if (unsupported.Any())
        {
            throw new SyntaxErrorException(
                $"The '{symbol}' operator does not support the following modifier(s): {string.Join(", ", unsupported)}",
                token);
        }

        return new(@operator) { Modifiers = modifiers };
    }

    private HashSet<string> ParseModifiers()
    {
        var modifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (_tokenizer.PeekNonWhitespaceToken().Type == SyntaxTokenType.Colon)
        {
            _tokenizer.NextNonWhitespaceToken();

            var token = _tokenizer.NextNonWhitespaceToken();
            if (token.Type != SyntaxTokenType.Identifier)
            {
                throw new SyntaxErrorException($"Expected a modifier after colon, but got: {token.Value}", token);
            }

            modifiers.Add(token.Value);
        }

        return modifiers;
    }
}
