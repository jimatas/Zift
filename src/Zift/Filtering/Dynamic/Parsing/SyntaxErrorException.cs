namespace Zift.Filtering.Dynamic.Parsing;

public class SyntaxErrorException : FormatException
{
    public SyntaxErrorException(string message)
        : base(message)
    {
    }

    public SyntaxErrorException(string message, SyntaxToken token)
        : base(FormatMessageWithTokenDetails(message, token))
    {
        Token = token;
    }

    public SyntaxToken? Token { get; }

    private static string FormatMessageWithTokenDetails(string message, SyntaxToken token)
    {
        return $"{message}\r\n(Token: {token.Type}, Value: {token.Value}, Position: {token.Position})";
    }
}
