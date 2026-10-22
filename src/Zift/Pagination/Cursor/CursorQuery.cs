namespace Zift.Pagination.Cursor;

using Ordering;

internal sealed class CursorQuery<T> :
    ICursorQuery<T>,
    IOrderedCursorQuery<T>
{
    internal CursorQuery(IQueryable<T> source, NullOrdering nullOrdering)
        : this(source, CursorQueryState<T>.Create(nullOrdering))
    {
    }

    private CursorQuery(IQueryable<T> source, CursorQueryState<T> state)
    {
        Source = source;
        State = state;
    }

    public IQueryable<T> Source { get; }

    public CursorQueryState<T> State { get; }

    public IOrderedCursorQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        return WithOrdering([new(keySelector, OrderingDirection.Ascending)]);
    }

    public IOrderedCursorQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        return WithOrdering([new(keySelector, OrderingDirection.Descending)]);
    }

    public IOrderedCursorQuery<T> OrderBy(string orderByClause, OrderingOptions? options = null)
    {
        return WithOrdering(ParseOrdering(orderByClause, options));
    }

    public IOrderedCursorQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        return WithOrdering([new(keySelector, OrderingDirection.Ascending)], append: true);
    }

    public IOrderedCursorQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        return WithOrdering([new(keySelector, OrderingDirection.Descending)], append: true);
    }

    public IOrderedCursorQuery<T> ThenBy(string orderByClause, OrderingOptions? options = null)
    {
        return WithOrdering(ParseOrdering(orderByClause, options), append: true);
    }

    public IExecutableCursorQuery<T> After(string? cursor)
    {
        return WithCursor(cursor, CursorNavigationMode.After);
    }

    public IExecutableCursorQuery<T> Before(string? cursor)
    {
        return WithCursor(cursor, CursorNavigationMode.Before);
    }

    private static IReadOnlyList<OrderingClause<T>> ParseOrdering(string orderByClause, OrderingOptions? options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderByClause);

        return OrderingParser.Parse<T>(orderByClause, options ?? new OrderingOptions());
    }

    private CursorQuery<T> WithOrdering(IReadOnlyList<OrderingClause<T>> clauses, bool append = false)
    {
        var ordering = append
            ? State.Ordering.Concat(clauses).ToArray()
            : clauses;

        return new(Source, State with { Ordering = ordering });
    }

    private CursorQuery<T> WithCursor(string? cursor, CursorNavigationMode navigationMode)
    {
        if (cursor is null)
        {
            return new(Source, State with { Cursor = null, NavigationMode = CursorNavigationMode.None });
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cursor);

        return new(Source, State with { Cursor = cursor, NavigationMode = navigationMode });
    }
}
