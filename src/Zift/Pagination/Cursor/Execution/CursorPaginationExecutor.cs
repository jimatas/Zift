namespace Zift.Pagination.Cursor.Execution;

using Expressions;
using Ordering;

internal static class CursorPaginationExecutor<T>
{
    private static readonly ConcurrentDictionary<LambdaExpression, Func<T, object?>>
        _cursorValueSelectorCache = [];

    public static CursorPage<T> Execute(
        IQueryable<T> source,
        CursorQueryState<T> state,
        int pageSize,
        Func<IQueryable<T>, int, List<T>> materializeItems)
    {
        var query = PrepareQuery(source, state);
        var items = materializeItems(query, pageSize + 1);

        return BuildPage(items, state, pageSize);
    }

    public static async Task<CursorPage<T>> ExecuteAsync(
        IQueryable<T> source,
        CursorQueryState<T> state,
        int pageSize,
        Func<IQueryable<T>, int, CancellationToken, Task<List<T>>> materializeItemsAsync,
        CancellationToken cancellationToken)
    {
        var query = PrepareQuery(source, state);
        var items = await materializeItemsAsync(query, pageSize + 1, cancellationToken)
            .ConfigureAwait(false);

        return BuildPage(items, state, pageSize);
    }

    private static IQueryable<T> PrepareQuery(
        IQueryable<T> source,
        CursorQueryState<T> state)
    {
        var isBackward = state.Direction == CursorDirection.Before;

        var ordering = isBackward
            ? state.Ordering.Reverse()
            : state.Ordering;

        IQueryable<T> query = ordering.ApplyTo(source);

        if (state.Direction != CursorDirection.None)
        {
            var predicate = BuildCursorPredicate(ordering, DecodeCursor(state));
            query = query.Where(predicate);
        }

        return query;
    }

    private static CursorValues DecodeCursor(CursorQueryState<T> state)
    {
        var cursorValueTypes = state.Ordering.Clauses
            .Select(c => c.KeySelector.ReturnType)
            .ToArray();

        return CursorValues.Decode(state.Cursor!, cursorValueTypes);
    }

    private static Expression<Func<T, bool>> BuildCursorPredicate(
        Ordering<T> ordering,
        CursorValues cursor)
    {
        var clauses = ordering.Clauses;
        var selectors = clauses.Select(clause => clause.KeySelector).ToArray();

        var rootParameter = selectors[0].Parameters[0];
        var normalizedSelectorBodies = selectors
            .Select(selector =>
                selector.Body.ReplaceParameter(
                    selector.Parameters[0],
                    rootParameter))
            .ToArray();

        Expression? predicate = null;

        for (var i = 0; i < clauses.Count; i++)
        {
            var keysetPredicate = BuildKeysetPredicate(i);

            predicate = predicate is null
                ? keysetPredicate
                : Expression.OrElse(predicate, keysetPredicate);
        }

        return Expression.Lambda<Func<T, bool>>(predicate!, rootParameter);

        Expression BuildKeysetPredicate(int index)
        {
            var clause = clauses[index];
            var selectorBody = normalizedSelectorBodies[index];

            var condition =
                ComparisonBuilder.BuildComparison(
                    selectorBody,
                    cursor.Values[index],
                    clause.Direction);

            for (var j = 0; j < index; j++)
            {
                var equality =
                    ComparisonBuilder.BuildEquality(
                        normalizedSelectorBodies[j],
                        cursor.Values[j]);

                condition = Expression.AndAlso(equality, condition);
            }

            return condition;
        }
    }

    private static CursorPage<T> BuildPage(
        List<T> items,
        CursorQueryState<T> state,
        int pageSize)
    {
        var hasExtraItem = items.Count > pageSize;

        var isBackward = state.Direction == CursorDirection.Before;
        var isExplicitForward = state.Direction == CursorDirection.After;

        if (hasExtraItem)
        {
            items.RemoveAt(items.Count - 1);
        }

        if (isBackward)
        {
            items.Reverse();
        }

        string? startCursor = null;
        string? endCursor = null;

        var hasNextPage = false;
        var hasPreviousPage = false;

        if (items.Count > 0)
        {
            startCursor = ExtractCursorValues(items[0], state.Ordering).Encode();
            endCursor = ExtractCursorValues(items[^1], state.Ordering).Encode();

            hasNextPage = isBackward || hasExtraItem;
            hasPreviousPage = isExplicitForward || (isBackward && hasExtraItem);
        }

        return new CursorPage<T>(
            items,
            startCursor,
            endCursor,
            hasNextPage,
            hasPreviousPage);
    }

    private static CursorValues ExtractCursorValues(T item, Ordering<T> ordering)
    {
        var values = new object?[ordering.Clauses.Count];

        for (var i = 0; i < ordering.Clauses.Count; i++)
        {
            var cursorValueSelector = GetOrCreateCursorValueSelector(
                ordering.Clauses[i].KeySelector);

            values[i] = cursorValueSelector(item);
        }

        return new CursorValues(values);
    }

    private static Func<T, object?> GetOrCreateCursorValueSelector(LambdaExpression keySelector) =>
        _cursorValueSelectorCache.GetOrAdd(
            keySelector,
            static lambda =>
            {
                var parameter = lambda.Parameters[0];
                var body = Expression.Convert(lambda.Body, typeof(object));

                return Expression.Lambda<Func<T, object?>>(body, parameter).Compile();
            });
}
