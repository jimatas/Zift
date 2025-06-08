namespace Zift.Filtering.Dynamic.Parsing;

/// <summary>
/// Provides extension methods for interpreting syntax tokens as typed values or operators.
/// </summary>
public static class SyntaxTokenExtensions
{
    /// <summary>
    /// Converts the token value to a <see cref="LogicalOperator"/>.
    /// </summary>
    /// <param name="token">The logical operator token (e.g., <c>"&amp;&amp;"</c> or <c>"||"</c>).</param>
    /// <returns>The corresponding <see cref="LogicalOperator"/>.</returns>
    /// <exception cref="SyntaxErrorException">Thrown if the token value is not a valid logical operator.</exception>
    public static LogicalOperator ToLogicalOperator(this SyntaxToken token)
    {
        return token.Value switch
        {
            "&&" => LogicalOperator.And,
            "||" => LogicalOperator.Or,
            _ => throw new SyntaxErrorException($"Expected a logical operator, but got: {token.Value}", token)
        };
    }

    /// <summary>
    /// Converts the token value to a <see cref="ComparisonOperatorType"/>.
    /// </summary>
    /// <param name="token">The comparison operator token (e.g., <c>"=="</c>, <c>"in"</c>).</param>
    /// <returns>The corresponding <see cref="ComparisonOperatorType"/>.</returns>
    /// <exception cref="SyntaxErrorException">Thrown if the token value is not a valid comparison operator.</exception>
    public static ComparisonOperatorType ToComparisonOperator(this SyntaxToken token)
    {
        if (ComparisonOperatorType.TryParse(token.Value, out var @operator))
        {
            return @operator;
        }

        throw new SyntaxErrorException($"Expected a comparison operator, but got: {token.Value}", token);
    }

    /// <summary>
    /// Converts a keyword, string literal, or numeric literal token to its typed value.
    /// </summary>
    /// <param name="token">The token to convert.</param>
    /// <returns>A typed representation of the token’s value.</returns>
    /// <exception cref="SyntaxErrorException">Thrown if the token type is invalid or cannot be parsed.</exception>
    public static object? ToTypedValue(this SyntaxToken token)
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
        return Math.Floor(number) == number &&
            !literal.Contains('.') &&
            !literal.Contains('E', StringComparison.OrdinalIgnoreCase);
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
