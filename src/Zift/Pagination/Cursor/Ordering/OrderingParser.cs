namespace Zift.Pagination.Cursor.Ordering;

using Expressions;

internal sealed class OrderingParser<T>(OrderingOptions options)
{
    private readonly OrderingOptions _options = options;

    public IReadOnlyList<OrderingClause<T>> Parse(string orderByClause)
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

    private OrderingClause<T> ParseClause(string clauseText)
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

    private LambdaExpression BuildKeySelector(string propertyPath)
    {
        var parameter = Expression.Parameter(
            typeof(T),
            ParameterName.FromType<T>());

        var propertyAccess = GuardedPropertyAccessBuilder.Build(
            parameter,
            propertyPath.Split('.'),
            enableNullGuards: _options.EnableNullGuards);

        var body = propertyAccess.NullGuard is { } nullGuard
            ? Expression.Condition(
                nullGuard,
                propertyAccess.Value,
                Expression.Default(propertyAccess.Value.Type))
            : propertyAccess.Value;

        return Expression.Lambda(body, parameter);
    }
}
