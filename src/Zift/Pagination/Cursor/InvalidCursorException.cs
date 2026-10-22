namespace Zift.Pagination.Cursor;

public sealed class InvalidCursorException : FormatException
{
    public InvalidCursorException()
        : base("The cursor is invalid.")
    {
    }

    public InvalidCursorException(string message)
        : base(message)
    {
    }

    public InvalidCursorException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
