namespace Zift.Filtering.Dynamic.Parsing;

/// <summary>
/// Represents a syntax error encountered during dynamic filter expression parsing.
/// </summary>
public class SyntaxErrorException : FormatException
{
    internal const int MaxTokenValueLength = 100;

    /// <summary>
    /// Initializes a new instance of <see cref="SyntaxErrorException"/> with a specified error message.
    /// </summary>
    /// <param name="message">The error message describing the parsing issue.</param>
    public SyntaxErrorException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SyntaxErrorException"/> with a specified error message and token context.
    /// </summary>
    /// <param name="message">The error message describing the parsing issue.</param>
    /// <param name="token">The token that caused the error.</param>
    public SyntaxErrorException(string message, SyntaxToken token)
        : base(FormatMessageWithTokenDetails(message, token)) => Token = token;

    /// <summary>
    /// The syntax token that caused the error, if available.
    /// </summary>
    public SyntaxToken? Token { get; }

    private static string FormatMessageWithTokenDetails(string message, SyntaxToken token)
    {
        var value = token.Value ?? string.Empty;
        if (value.Length > MaxTokenValueLength)
        {
            value = $"{value[..(MaxTokenValueLength - 3)]}...";
        }

        return $"{message}{Environment.NewLine}" +
            $"(Token: {token.Type}, Value: {value}, Position: {token.Position})";
    }
}
