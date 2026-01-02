namespace Zift.Pagination.Cursor.Ordering;

internal static class OrderingParser<T>
{
    public static IReadOnlyList<OrderingClause<T>> Parse(string orderByClause)
    {
        var clauses = new List<OrderingClause<T>>();

        foreach (var clauseText in orderByClause.Split(',', StringSplitOptions.TrimEntries))
        {
            if (string.IsNullOrEmpty(clauseText))
            {
                throw new FormatException(
                    "Order-by expression contains an empty clause.");
            }

            clauses.Add(ParseClause(clauseText));
        }

        return clauses;
    }

    private static OrderingClause<T> ParseClause(string clauseText)
    {
        var clauseParts = clauseText.Split(
            Array.Empty<char>(),
            StringSplitOptions.RemoveEmptyEntries);

        var propertyPath = clauseParts[0];

        var direction = clauseParts.Length switch
        {
            1 => OrderingDirection.Ascending,
            2 => ParseDirection(clauseParts[1]),
            _ => throw new FormatException(
                $"Invalid order-by clause '{clauseText}'. Expected format is 'Property [ASC|DESC]'.")
        };

        var keySelector = BuildKeySelector(propertyPath);

        return OrderingClause<T>.Create(keySelector, direction);
    }

    private static OrderingDirection ParseDirection(string directionText) =>
        directionText.ToUpperInvariant() switch
        {
            "ASC" => OrderingDirection.Ascending,
            "DESC" => OrderingDirection.Descending,
            _ => throw new FormatException(
                $"Invalid order-by direction '{directionText}'. Must be 'ASC' or 'DESC'.")
        };

    private static LambdaExpression BuildKeySelector(string propertyPath)
    {
        var parameter = Expression.Parameter(
            typeof(T),
            ParameterName.FromType<T>());

        Expression current = parameter;

        foreach (var property in propertyPath.Split('.'))
        {
            current = Expression.Property(current, property);
        }

        return Expression.Lambda(current, parameter);
    }
}
