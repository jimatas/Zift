namespace Zift.Pagination.Cursor.Ordering;

internal sealed class OrderingClause<T>
{
    public OrderingClause(LambdaExpression keySelector, OrderingDirection direction)
    {
        ArgumentNullException.ThrowIfNull(keySelector);

        if (keySelector.Parameters.Count != 1 || keySelector.Parameters[0].Type != typeof(T))
        {
            throw new ArgumentException(
                $"The key selector must take a single {typeof(T).Name} parameter.", nameof(keySelector));
        }

        if (keySelector.ReturnType == typeof(object))
        {
            throw new ArgumentException(
                "The key selector must return a strongly typed key (not object), so cursor values can be compared.",
                nameof(keySelector));
        }

        KeySelector = NullableLifting.LiftNavigationPath(keySelector);
        Direction = direction;
    }

    public LambdaExpression KeySelector { get; }

    public OrderingDirection Direction { get; }

    public Type KeyType => KeySelector.ReturnType;

    public OrderingClause<T> Reverse()
    {
        var reversedDirection = Direction == OrderingDirection.Ascending
            ? OrderingDirection.Descending
            : OrderingDirection.Ascending;

        return new(KeySelector, reversedDirection);
    }

    public IOrderedQueryable<T> ApplyTo(IQueryable<T> query)
    {
        var methodName = Direction == OrderingDirection.Ascending
            ? nameof(Queryable.OrderBy)
            : nameof(Queryable.OrderByDescending);

        return Apply(query, methodName);
    }

    public IOrderedQueryable<T> ApplyTo(IOrderedQueryable<T> query)
    {
        var methodName = Direction == OrderingDirection.Ascending
            ? nameof(Queryable.ThenBy)
            : nameof(Queryable.ThenByDescending);

        return Apply(query, methodName);
    }

    public string Describe()
    {
        var canonical = Expression.Parameter(typeof(T), ParameterName.Default);
        var body = KeySelector.Body.ReplaceParameter(KeySelector.Parameters[0], canonical);

        return $"{body.Describe()}:{Direction}";
    }

    private IOrderedQueryable<T> Apply(IQueryable<T> query, string method)
    {
        var call = Expression.Call(
            typeof(Queryable),
            method,
            [typeof(T), KeyType],
            query.Expression,
            Expression.Quote(KeySelector));

        return (IOrderedQueryable<T>)query.Provider.CreateQuery<T>(call);
    }
}
