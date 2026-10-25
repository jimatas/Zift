namespace Zift.Pagination.Cursor;

public sealed class InvalidOrderingClauseException : FormatException
{
    public InvalidOrderingClauseException()
        : base("The order-by clause is invalid.")
    {
    }

    public InvalidOrderingClauseException(string message)
        : base(message)
    {
    }

    public InvalidOrderingClauseException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    internal static InvalidOrderingClauseException KeyLimitExceeded(int maxOrderingKeys)
    {
        return new($"The ordering exceeds the maximum of {maxOrderingKeys} keys.");
    }
}
