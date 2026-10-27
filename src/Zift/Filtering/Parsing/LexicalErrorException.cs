namespace Zift.Filtering.Parsing;

public sealed class LexicalErrorException : InvalidFilterException
{
    internal LexicalErrorException(string message, int position)
        : base(FormatErrorMessage(message, position), position)
    {
    }

    private static string FormatErrorMessage(string message, int position)
    {
        return $"{message}{Environment.NewLine}(Position: {position})";
    }
}
