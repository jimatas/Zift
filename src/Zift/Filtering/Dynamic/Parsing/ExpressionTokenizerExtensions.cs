namespace Zift.Filtering.Dynamic.Parsing;

/// <summary>
/// Provides extension methods for working with <see cref="ExpressionTokenizer"/> instances.
/// </summary>
public static class ExpressionTokenizerExtensions
{
    /// <summary>
    /// Retrieves the next non-whitespace token from the tokenizer.
    /// </summary>
    public static SyntaxToken NextNonWhitespaceToken(this ExpressionTokenizer tokenizer)
    {
        var token = tokenizer.NextToken();
        if (token.Type == SyntaxTokenType.Whitespace)
        {
            token = tokenizer.NextToken();
        }

        return token;
    }

    /// <summary>
    /// Peeks at the next non-whitespace token without advancing the tokenizer.
    /// </summary>
    public static SyntaxToken PeekNonWhitespaceToken(this ExpressionTokenizer tokenizer)
    {
        var token = tokenizer.PeekToken();
        if (token.Type != SyntaxTokenType.Whitespace)
        {
            return token;
        }

        tokenizer.NextToken();

        return tokenizer.PeekToken();
    }
}
