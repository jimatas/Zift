namespace Zift.Pagination.Cursor.Ordering;

internal abstract class OrderingClause<T>
{
    private static readonly ConcurrentDictionary<Type, Func<LambdaExpression, OrderingDirection, OrderingClause<T>>>
        _typedFactoryCache = [];

    public abstract LambdaExpression KeySelector { get; }
    public abstract OrderingDirection Direction { get; }

    public abstract OrderingClause<T> Reverse();

    public abstract IOrderedQueryable<T> ApplyTo(IQueryable<T> query);
    public abstract IOrderedQueryable<T> ApplyTo(IOrderedQueryable<T> orderedQuery);

    public static OrderingClause<T> Create(
        LambdaExpression keySelector,
        OrderingDirection direction)
    {
        var keyType = keySelector.ReturnType;

        var factory = _typedFactoryCache.GetOrAdd(
            keyType,
            static keyType => CreateTypedFactory(keyType));

        return factory(keySelector, direction);
    }

    private static Func<LambdaExpression, OrderingDirection, OrderingClause<T>> CreateTypedFactory(Type keyType)
    {
        var clauseType =
            typeof(OrderingClause<,>).MakeGenericType(typeof(T), keyType);

        var ctor = clauseType.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types:
            [
                typeof(Expression<>).MakeGenericType(
                    typeof(Func<,>).MakeGenericType(typeof(T), keyType)),
                typeof(OrderingDirection)
            ],
            modifiers: null)!;

        return (keySelector, direction) =>
            (OrderingClause<T>)ctor.Invoke([keySelector, direction])!;
    }
}
