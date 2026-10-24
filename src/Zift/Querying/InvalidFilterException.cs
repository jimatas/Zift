namespace Zift.Querying;

public class InvalidFilterException : FormatException
{
    public InvalidFilterException(string message)
        : base(message)
    {
    }

    public InvalidFilterException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public InvalidFilterException(string message, int position)
        : base(message)
    {
        Position = position;
    }

    public int? Position { get; }
}
