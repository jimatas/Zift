namespace Zift.Pagination.Cursor.Ordering;

internal static class OrderingParser
{
    public static IReadOnlyList<OrderingClause<T>> Parse<T>(
        string orderByClause,
        OrderingOptions options)
    {
        var clauses = new List<OrderingClause<T>>();

        foreach (var text in orderByClause.Split(',', StringSplitOptions.TrimEntries))
        {
            if (text.Length == 0)
            {
                throw new InvalidOrderingClauseException("The order-by clause contains an empty entry.");
            }

            clauses.Add(ParseEntry<T>(text, options));
        }

        return clauses;
    }

    private static OrderingClause<T> ParseEntry<T>(string text, OrderingOptions options)
    {
        var parts = text.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);

        var direction = parts.Length switch
        {
            1 => OrderingDirection.Ascending,
            2 => ParseDirection(parts[1]),
            _ => throw new InvalidOrderingClauseException(
                $"Invalid order-by entry '{text}'. Expected 'Property [ASC|DESC]'.")
        };

        var keySelector = PropertyPathResolver.Resolve<T>(
            parts[0],
            options.EnableNullGuards);

        return new OrderingClause<T>(keySelector, direction);
    }

    private static OrderingDirection ParseDirection(string text)
    {
        return text.ToUpperInvariant() switch
        {
            "ASC" => OrderingDirection.Ascending,
            "DESC" => OrderingDirection.Descending,
            _ => throw new InvalidOrderingClauseException(
                $"Invalid order-by direction '{text}'. Must be 'ASC' or 'DESC'.")
        };
    }
}
