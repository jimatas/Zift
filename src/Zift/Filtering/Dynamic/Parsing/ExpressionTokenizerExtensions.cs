namespace Zift.Filtering.Dynamic.Parsing;

public static class ExpressionTokenizerExtensions
{
    public static SyntaxToken NextNonWhitespaceToken(this ExpressionTokenizer tokenizer)
    {
        var token = tokenizer.NextToken();
        if (token.Type == SyntaxTokenType.Whitespace)
        {
            token = tokenizer.NextToken();
        }

        return token;
    }

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
