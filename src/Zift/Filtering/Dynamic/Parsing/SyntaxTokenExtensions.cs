namespace Zift.Filtering.Dynamic.Parsing;

public static class SyntaxTokenExtensions
{
    public static LogicalOperator ToLogicalOperator(this SyntaxToken token)
    {
        return token.Value switch
        {
            "&&" => LogicalOperator.And,
            "||" => LogicalOperator.Or,
            _ => throw new SyntaxErrorException($"Expected a logical operator, but got: {token.Value}", token)
        };
    }
    
    public static ComparisonOperator ToComparisonOperator(this SyntaxToken token)
    {
        return token.Value switch
        {
            "==" => ComparisonOperator.Equal,
            "!=" => ComparisonOperator.NotEqual,
            ">" => ComparisonOperator.GreaterThan,
            ">=" => ComparisonOperator.GreaterThanOrEqual,
            "<" => ComparisonOperator.LessThan,
            "<=" => ComparisonOperator.LessThanOrEqual,
            "%=" => ComparisonOperator.Contains,
            "^=" => ComparisonOperator.StartsWith,
            "$=" => ComparisonOperator.EndsWith,
            _ => throw new SyntaxErrorException($"Expected a comparison operator, but got: {token.Value}", token)
        };
    }

    public static LiteralValue ToLiteralValue(this SyntaxToken valueToken, SyntaxToken? modifierToken = null)
    {
        var rawValue = valueToken.ToTypedValue();
        var modifier = ParseStringModifier(valueToken, modifierToken);

        return new(rawValue) { Modifier = modifier };
    }

    private static StringValueModifier? ParseStringModifier(SyntaxToken valueToken, SyntaxToken? modifierToken)
    {
        if (modifierToken is not { Value: { } modifierSymbol })
        {
            return null;
        }

        if (valueToken.Type != SyntaxTokenType.StringLiteral)
        {
            throw new SyntaxErrorException("Modifiers are only supported on string literals.", modifierToken.Value);
        }

        try
        {
            return StringValueModifierExtensions.FromSymbol(modifierSymbol);
        }
        catch (ArgumentException)
        {
            throw new SyntaxErrorException($"Unsupported modifier: {modifierSymbol}", modifierToken.Value);
        }
    }

    private static object? ToTypedValue(this SyntaxToken token)
    {
        return token.Type switch
        {
            SyntaxTokenType.Keyword => token.ToKeywordValue(),
            SyntaxTokenType.StringLiteral => token.ToStringValue(),
            SyntaxTokenType.NumericLiteral => token.ToNumericValue(),
            _ => throw new SyntaxErrorException("Unexpected token type while parsing value. "
                + "Expected a keyword, a string literal, or a numeric literal.", token)
        };
    }

    private static object? ToKeywordValue(this SyntaxToken token)
    {
        return token.Value.ToLowerInvariant() switch
        {
            "true" => true,
            "false" => false,
            "null" => null,
            _ => throw new SyntaxErrorException($"Unsupported keyword: {token.Value}", token)
        };
    }

    private static object? ToNumericValue(this SyntaxToken token)
    {
        if (double.TryParse(token.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var number))
        {
            if (IsIntegralWithoutScientificNotation(token.Value, number))
            {
                return (int)number;
            }

            return number;
        }

        throw new SyntaxErrorException($"Invalid numeric format: {token.Value}", token);
    }

    private static bool IsIntegralWithoutScientificNotation(string literal, double number)
    {
        return Math.Floor(number) == number
            && !literal.Contains('.')
            && !literal.Contains('E', StringComparison.OrdinalIgnoreCase);
    }

    private static object? ToStringValue(this SyntaxToken token)
    {
        var literal = token.Value;
        if (IsProperlyQuoted(literal))
        {
            literal = UnescapeQuotedString(literal);
        }

        return literal;
    }

    private static bool IsProperlyQuoted(string literal)
    {
        return literal.Length >= 2 && literal[0] is '"' or '\'' && literal[0] == literal[^1];
    }

    private static string UnescapeQuotedString(string literal)
    {
        var quoteChar = literal[0];

        return literal[1..^1].Replace($"\\{quoteChar}", quoteChar.ToString());
    }
}
