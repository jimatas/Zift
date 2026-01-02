namespace Zift.Pagination.Cursor.Ordering;

internal sealed class OrderingClause<T, TKey>(
    Expression<Func<T, TKey>> keySelector,
    OrderingDirection direction) : OrderingClause<T>
{
    public override Expression<Func<T, TKey>> KeySelector { get; } = keySelector;
    public override OrderingDirection Direction { get; } = direction;

    public override OrderingClause<T, TKey> Reverse() =>
        new(KeySelector,
            Direction == OrderingDirection.Ascending
                ? OrderingDirection.Descending
                : OrderingDirection.Ascending);

    public override IOrderedQueryable<T> ApplyTo(IQueryable<T> query) =>
        Direction == OrderingDirection.Ascending
            ? query.OrderBy(KeySelector)
            : query.OrderByDescending(KeySelector);

    public override IOrderedQueryable<T> ApplyTo(IOrderedQueryable<T> orderedQuery) =>
        Direction == OrderingDirection.Ascending
            ? orderedQuery.ThenBy(KeySelector)
            : orderedQuery.ThenByDescending(KeySelector);
}
