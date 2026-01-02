namespace Zift.Querying.Parsing;

public sealed class LexicalErrorException(string message, int position)
    : FormatException(FormatErrorMessage(message, position))
{
    internal int Position { get; } = position;

    private static string FormatErrorMessage(string message, int position) =>
        $"{message}{Environment.NewLine}(Position: {position})";
}
