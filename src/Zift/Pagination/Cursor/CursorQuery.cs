namespace Zift.Pagination.Cursor;

using Ordering;

internal sealed class CursorQuery<T> :
    ICursorQuery<T>,
    IOrderedCursorQuery<T>,
    IExecutableCursorQuery<T>,
    ICursorQueryState<T>
{
    internal CursorQuery(IQueryable<T> source)
    {
        Source = source;
        State = CursorQueryState<T>.Empty;
    }

    private CursorQuery(
        IQueryable<T> source,
        CursorQueryState<T> state)
    {
        Source = source;
        State = state;
    }

    public IQueryable<T> Source { get; }
    public CursorQueryState<T> State { get; }

    public IOrderedCursorQuery<T> OrderBy<TKey>(
        Expression<Func<T, TKey>> keySelector) =>
        WithOrdering(keySelector, OrderingDirection.Ascending);

    public IOrderedCursorQuery<T> OrderByDescending<TKey>(
        Expression<Func<T, TKey>> keySelector) =>
        WithOrdering(keySelector, OrderingDirection.Descending);

    public IOrderedCursorQuery<T> OrderBy(
        string orderByClause,
        OrderingOptions? options = null)
    {
        var ordering = Ordering<T>.Parse(
            orderByClause,
            options ?? new OrderingOptions());

        if (ordering.IsEmpty)
        {
            throw new ArgumentException(
                "The order-by expression must contain at least one ordering clause.",
                nameof(orderByClause));
        }

        return WithState(State with
        {
            Ordering = ordering
        });
    }

    public IOrderedCursorQuery<T> ThenBy<TKey>(
        Expression<Func<T, TKey>> keySelector) =>
        WithOrdering(keySelector, OrderingDirection.Ascending);

    public IOrderedCursorQuery<T> ThenByDescending<TKey>(
        Expression<Func<T, TKey>> keySelector) =>
        WithOrdering(keySelector, OrderingDirection.Descending);

    public IExecutableCursorQuery<T> After(string cursor) =>
        WithCursor(cursor, CursorDirection.After);

    public IExecutableCursorQuery<T> Before(string cursor) =>
        WithCursor(cursor, CursorDirection.Before);

    private CursorQuery<T> WithOrdering<TKey>(
        Expression<Func<T, TKey>> keySelector,
        OrderingDirection direction)
    {
        ArgumentNullException.ThrowIfNull(keySelector);

        var clause = new OrderingClause<T, TKey>(keySelector, direction);

        return WithState(
            State with
            {
                Ordering = State.Ordering.Append(clause)
            });
    }

    private CursorQuery<T> WithCursor(
        string? cursor,
        CursorDirection direction)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cursor);

        return WithState(
            State with
            {
                Direction = direction,
                Cursor = cursor
            });
    }

    private CursorQuery<T> WithState(CursorQueryState<T> state) =>
        new(Source, state);
}
