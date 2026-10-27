using Zift.Pagination.Cursor.Ordering;

namespace Zift.Pagination.Cursor.Execution;

internal static class CursorPaginationExecutor<T>
{
    private const int MaxPageSize = int.MaxValue - 1;

    public static CursorPage<T> Execute(
        IExecutableCursorQuery<T> query,
        int pageSize,
        Func<IQueryable<KeyedItem<T>>, List<KeyedItem<T>>> materialize)
    {
        var (source, state) = ValidateAndUnwrap(query, pageSize);
        var preparedQuery = PrepareQuery(source, state, pageSize);
        var rows = materialize(preparedQuery);

        return BuildPage(rows, state, pageSize);
    }

    public static Task<CursorPage<T>> ExecuteAsync(
        IExecutableCursorQuery<T> query,
        int pageSize,
        Func<IQueryable<KeyedItem<T>>, Task<List<KeyedItem<T>>>> materializeAsync)
    {
        var (source, state) = ValidateAndUnwrap(query, pageSize);
        var preparedQuery = PrepareQuery(source, state, pageSize);

        return ExecuteCoreAsync(preparedQuery, state, pageSize, materializeAsync);
    }

    private static async Task<CursorPage<T>> ExecuteCoreAsync(
        IQueryable<KeyedItem<T>> query,
        CursorQueryState<T> state,
        int pageSize,
        Func<IQueryable<KeyedItem<T>>, Task<List<KeyedItem<T>>>> materializeAsync)
    {
        var rows = await materializeAsync(query).ConfigureAwait(false);

        return BuildPage(rows, state, pageSize);
    }

    private static (IQueryable<T> Source, CursorQueryState<T> State) ValidateAndUnwrap(
        IExecutableCursorQuery<T> query,
        int pageSize)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pageSize, MaxPageSize);

        if (query is not CursorQuery<T> cursorQuery)
        {
            throw new InvalidOperationException("The cursor query was not created by AsCursorQuery().");
        }

        return (cursorQuery.Source, cursorQuery.State);
    }

    private static IQueryable<KeyedItem<T>> PrepareQuery(
        IQueryable<T> source,
        CursorQueryState<T> state,
        int pageSize)
    {
        if (state.Ordering.Count == 0)
        {
            throw new InvalidOperationException("Cursor pagination requires at least one ordering clause.");
        }

        var isBackward = state.NavigationMode == CursorNavigationMode.Before;
        var ordering = isBackward
            ? state.Ordering.Select(clause => clause.Reverse()).ToArray()
            : state.Ordering;

        var rootParameter = ordering[0].KeySelector.Parameters[0];
        var normalizedSelectorBodies = NormalizeSelectorParameters(ordering, rootParameter);

        IQueryable<T> query = ApplyOrdering(source, ordering);

        if (state.NavigationMode != CursorNavigationMode.None)
        {
            var cursorValues = DecodeCursor(state);
            var predicate = BuildKeysetPredicate(
                rootParameter,
                normalizedSelectorBodies,
                ordering,
                cursorValues,
                state.NullOrdering);

            query = query.Where(predicate);
        }

        var projection = BuildKeyedItemProjection(rootParameter, normalizedSelectorBodies);

        return query.Select(projection).Take(pageSize + 1);
    }

    private static object?[] DecodeCursor(CursorQueryState<T> state)
    {
        var fingerprint = ComputeOrderingFingerprint(state);
        var keyTypes = state.Ordering.Select(clause => clause.KeyType).ToArray();

        return CursorToken.Decode(state.Cursor!, fingerprint, keyTypes);
    }

    private static IOrderedQueryable<T> ApplyOrdering(
        IQueryable<T> source,
        IReadOnlyList<OrderingClause<T>> ordering)
    {
        var query = ordering[0].ApplyTo(source);
        for (var i = 1; i < ordering.Count; i++)
        {
            query = ordering[i].ApplyTo(query);
        }

        return query;
    }

    private static Expression[] NormalizeSelectorParameters(
        IReadOnlyList<OrderingClause<T>> ordering,
        ParameterExpression rootParameter)
    {
        var normalizedSelectorBodies = new Expression[ordering.Count];
        for (var i = 0; i < ordering.Count; i++)
        {
            var selector = ordering[i].KeySelector;

            normalizedSelectorBodies[i] = selector.Body.ReplaceParameter(selector.Parameters[0], rootParameter);
        }

        return normalizedSelectorBodies;
    }

    private static Expression<Func<T, bool>> BuildKeysetPredicate(
        ParameterExpression rootParameter,
        Expression[] normalizedSelectorBodies,
        IReadOnlyList<OrderingClause<T>> ordering,
        object?[] cursorValues,
        NullOrdering nullOrdering)
    {
        Expression? predicate = null;

        for (var i = 0; i < ordering.Count; i++)
        {
            var condition = ComparisonBuilder.BuildComparison(
                normalizedSelectorBodies[i],
                cursorValues[i],
                ordering[i].Direction,
                nullOrdering);

            for (var j = i - 1; j >= 0; j--)
            {
                var equality = ComparisonBuilder.BuildEquality(
                    normalizedSelectorBodies[j],
                    cursorValues[j]);

                condition = Expression.AndAlso(equality, condition);
            }

            predicate = predicate is null
                ? condition
                : Expression.OrElse(predicate, condition);
        }

        if (ordering.Count > 1 &&
            ComparisonBuilder.BuildLeadingKeyBound(
                normalizedSelectorBodies[0],
                cursorValues[0],
                ordering[0].Direction,
                nullOrdering) is { } bound)
        {
            predicate = Expression.AndAlso(bound, predicate!);
        }

        return Expression.Lambda<Func<T, bool>>(predicate!, rootParameter);
    }

    private static Expression<Func<T, KeyedItem<T>>> BuildKeyedItemProjection(
        ParameterExpression rootParameter,
        Expression[] normalizedSelectorBodies)
    {
        var keyArray = Expression.NewArrayInit(
            typeof(object),
            normalizedSelectorBodies.Select(body => (Expression)Expression.Convert(body, typeof(object))));

        var constructor = typeof(KeyedItem<T>).GetConstructor([typeof(T), typeof(object[])])!;
        var body = Expression.New(constructor, rootParameter, keyArray);

        return Expression.Lambda<Func<T, KeyedItem<T>>>(body, rootParameter);
    }

    private static CursorPage<T> BuildPage(
        List<KeyedItem<T>> rows,
        CursorQueryState<T> state,
        int pageSize)
    {
        var isBackward = state.NavigationMode == CursorNavigationMode.Before;
        var isExplicitForward = state.NavigationMode == CursorNavigationMode.After;

        var hasExtraRow = rows.Count > pageSize;
        if (hasExtraRow)
        {
            rows.RemoveAt(rows.Count - 1);
        }

        if (isBackward)
        {
            rows.Reverse();
        }

        var items = rows.ConvertAll(row => row.Item);

        if (items.Count == 0)
        {
            return CursorPage<T>.Empty;
        }

        var fingerprint = ComputeOrderingFingerprint(state);
        var startCursor = CursorToken.Encode(fingerprint, rows[0].Keys);
        var endCursor = CursorToken.Encode(fingerprint, rows[^1].Keys);

        var hasNextPage = isBackward || hasExtraRow;
        var hasPreviousPage = isExplicitForward || (isBackward && hasExtraRow);

        return new CursorPage<T>(
            items,
            startCursor,
            endCursor,
            hasNextPage,
            hasPreviousPage);
    }

    private static string ComputeOrderingFingerprint(CursorQueryState<T> state)
    {
        var segments = state.Ordering
            .Select(clause => clause.Describe())
            .Prepend(typeof(T).FullName)
            .Append(DescribeNullOrdering(state.NullOrdering));

        var description = string.Join('|', segments);
        var descriptionBytes = Encoding.UTF8.GetBytes(description);
        var hash = SHA256.HashData(descriptionBytes);

        return Convert.ToHexString(hash, 0, 8);
    }

    private static string DescribeNullOrdering(NullOrdering nullOrdering)
    {
        return nullOrdering switch
        {
            NullOrdering.NullsCompareLower => "nulls-low",
            NullOrdering.NullsCompareHigher => "nulls-high",
            _ => throw new UnreachableException()
        };
    }
}
