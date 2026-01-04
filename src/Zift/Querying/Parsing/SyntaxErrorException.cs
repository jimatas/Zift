namespace Zift.Querying.Parsing;

public sealed class SyntaxErrorException(string message) : FormatException(message)
{
    internal SyntaxErrorException(string message, SyntaxToken token)
        : this(FormatErrorMessage(message, token)) => Token = token;

    internal SyntaxToken? Token { get; }

    private static string FormatErrorMessage(string message, SyntaxToken token)
    {
        var text = token.Text ?? string.Empty;

        const int maxTextLength = 100;
        if (text.Length > maxTextLength)
        {
            text = $"{text[..(maxTextLength - 3)]}...";
        }

        return $"{message}{Environment.NewLine}" +
            $"(Token: {token.Type}, Text: {text}, Position: {token.Position})";
    }
}
