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
        Expression<Func<T, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);

        var clause = new OrderingClause<T, TKey>(
            keySelector,
            OrderingDirection.Ascending);

        return new CursorQuery<T>(
            Source,
            State with
            {
                Ordering = State.Ordering.Append(clause)
            });
    }

    public IOrderedCursorQuery<T> OrderByDescending<TKey>(
        Expression<Func<T, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);

        var clause = new OrderingClause<T, TKey>(
            keySelector,
            OrderingDirection.Descending);

        return new CursorQuery<T>(
            Source,
            State with
            {
                Ordering = State.Ordering.Append(clause)
            });
    }

    public IOrderedCursorQuery<T> OrderBy(string orderByClause)
    {
        var ordering = Ordering<T>.Parse(orderByClause);

        if (ordering.IsEmpty)
        {
            throw new InvalidOperationException(
                "Order-by expression must contain at least one clause for cursor pagination.");
        }

        return new CursorQuery<T>(
            Source,
            State with { Ordering = ordering });
    }

    public IOrderedCursorQuery<T> ThenBy<TKey>(
        Expression<Func<T, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);

        var clause = new OrderingClause<T, TKey>(
            keySelector,
            OrderingDirection.Ascending);

        return new CursorQuery<T>(
            Source,
            State with
            {
                Ordering = State.Ordering.Append(clause)
            });
    }

    public IOrderedCursorQuery<T> ThenByDescending<TKey>(
        Expression<Func<T, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);

        var clause = new OrderingClause<T, TKey>(
            keySelector,
            OrderingDirection.Descending);

        return new CursorQuery<T>(
            Source,
            State with
            {
                Ordering = State.Ordering.Append(clause)
            });
    }

    public IExecutableCursorQuery<T> After(string cursor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cursor);

        return new CursorQuery<T>(
            Source,
            State with
            {
                Direction = CursorDirection.After,
                Cursor = cursor
            });
    }

    public IExecutableCursorQuery<T> Before(string cursor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cursor);

        return new CursorQuery<T>(
            Source,
            State with
            {
                Direction = CursorDirection.Before,
                Cursor = cursor
            });
    }
}
