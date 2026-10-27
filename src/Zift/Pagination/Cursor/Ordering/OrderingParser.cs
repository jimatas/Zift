namespace Zift.Pagination.Cursor.Ordering;

internal static class OrderingParser
{
    public static IReadOnlyList<OrderingClause<T>> Parse<T>(
        string orderByClause,
        bool enableNullGuards,
        int maxOrderingKeys,
        int existingOrderingKeys = 0)
    {
        var clauses = new List<OrderingClause<T>>();
        var remaining = orderByClause.AsSpan();

        while (true)
        {
            var separatorIndex = remaining.IndexOf(',');
            var entry = (separatorIndex < 0 ? remaining : remaining[..separatorIndex]).Trim();

            if (entry.IsEmpty)
            {
                throw new InvalidOrderingClauseException("The order-by clause contains an empty entry.");
            }

            if (existingOrderingKeys + clauses.Count >= maxOrderingKeys)
            {
                throw InvalidOrderingClauseException.KeyLimitExceeded(maxOrderingKeys);
            }

            clauses.Add(ParseEntry<T>(entry.ToString(), enableNullGuards));

            if (separatorIndex < 0)
            {
                return clauses;
            }

            remaining = remaining[(separatorIndex + 1)..];
        }
    }

    private static OrderingClause<T> ParseEntry<T>(string entry, bool enableNullGuards)
    {
        var parts = entry.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);

        var direction = parts.Length switch
        {
            1 => OrderingDirection.Ascending,
            2 => ParseDirection(parts[1]),
            _ => throw new InvalidOrderingClauseException(
                $"Invalid order-by entry '{entry}'. Expected 'Property [ASC|DESC]'.")
        };

        var keySelector = PropertyPathResolver.Resolve<T>(
            parts[0],
            enableNullGuards);

        return new OrderingClause<T>(keySelector, direction);
    }

    private static OrderingDirection ParseDirection(string direction)
    {
        return direction.ToUpperInvariant() switch
        {
            "ASC" => OrderingDirection.Ascending,
            "DESC" => OrderingDirection.Descending,
            _ => throw new InvalidOrderingClauseException(
                $"Invalid order-by direction '{direction}'. Must be 'ASC' or 'DESC'.")
        };
    }
}
