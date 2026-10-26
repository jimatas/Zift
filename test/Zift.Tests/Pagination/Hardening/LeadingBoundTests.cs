using Zift.Pagination.Cursor;

namespace Zift.Pagination.Tests;

public class LeadingBoundTests
{
    private enum Kind { Low, Mid, High }

    private sealed record Row(int Id, string? S, int? N, DateTime D, Guid G, decimal M, Kind K, bool B);

    private static readonly Row[] Rows = Enumerable.Range(1, 36).Select(i => new Row(
        i,
        i % 4 == 0 ? null : "s" + (i % 3),
        i % 5 == 0 ? null : i % 3,
        new DateTime(2020, 1, 1).AddDays(i % 4),
        new Guid(i % 3 + 1, 0, 0, new byte[8]),
        i % 4 * 1.5m,
        (Kind)(i % 3),
        i % 2 == 0)).ToArray();

    private static CursorQueryOptions Options(NullOrdering nullOrdering) => new() { NullOrdering = nullOrdering };


    [Fact]
    public void TwoKeys_NonNullCursor_AddsAnInclusiveBound_OutsideTheOrChain()
    {
        Func<IQueryable<Row>, IOrderedCursorQuery<Row>> order = s => s.AsCursorQuery().OrderBy(r => r.D).ThenBy(r => r.Id);
        var cursor = order(Rows.AsQueryable()).ToCursorPage(5).EndCursor;

        var after = PredicateOf(order, cursor, forward: true).Body;
        var before = PredicateOf(order, cursor, forward: false).Body;

        Assert.Equal(ExpressionType.AndAlso, after.NodeType);
        Assert.Equal(ExpressionType.GreaterThanOrEqual, ((BinaryExpression)after).Left.NodeType);
        Assert.Equal(ExpressionType.OrElse, ((BinaryExpression)after).Right.NodeType);
        Assert.Equal(ExpressionType.AndAlso, before.NodeType);
        Assert.Equal(ExpressionType.LessThanOrEqual, ((BinaryExpression)before).Left.NodeType);
    }

    [Fact]
    public void DescendingLeadingKey_FlipsTheBound()
    {
        Func<IQueryable<Row>, IOrderedCursorQuery<Row>> order = s => s.AsCursorQuery().OrderByDescending(r => r.D).ThenBy(r => r.Id);
        var cursor = order(Rows.AsQueryable()).ToCursorPage(5).EndCursor;

        Assert.Equal(ExpressionType.LessThanOrEqual,
            ((BinaryExpression)PredicateOf(order, cursor, forward: true).Body).Left.NodeType);
        Assert.Equal(ExpressionType.GreaterThanOrEqual,
            ((BinaryExpression)PredicateOf(order, cursor, forward: false).Body).Left.NodeType);
    }

    [Fact]
    public void SingleKey_HasNoBound()
    {
        Func<IQueryable<Row>, IOrderedCursorQuery<Row>> order = s => s.AsCursorQuery().OrderBy(r => r.Id);
        var cursor = order(Rows.AsQueryable()).ToCursorPage(5).EndCursor;

        Assert.Equal(0, CountInclusive(PredicateOf(order, cursor, forward: true)));
        Assert.Equal(0, CountInclusive(PredicateOf(order, cursor, forward: false)));
    }

    [Fact]
    public void ThreeKeys_BoundsOnlyTheLeadingKey()
    {
        Func<IQueryable<Row>, IOrderedCursorQuery<Row>> order =
            s => s.AsCursorQuery().OrderBy(r => r.D).ThenByDescending(r => r.M).ThenBy(r => r.Id);
        var cursor = order(Rows.AsQueryable()).ToCursorPage(5).EndCursor;

        Assert.Equal(1, CountInclusive(PredicateOf(order, cursor, forward: true)));
    }

    [Theory]
    [InlineData(NullOrdering.NullsCompareLower)]
    [InlineData(NullOrdering.NullsCompareHigher)]
    public void NullLeadingCursorValue_HasNoBound_UnderEachConvention(NullOrdering nullOrdering)
    {
        Func<IQueryable<Row>, IOrderedCursorQuery<Row>> order =
            s => s.AsCursorQuery(Options(nullOrdering)).OrderBy(r => r.S).ThenBy(r => r.Id);
        var source = NullOrderedSource.Create(Rows, nullOrdering);
        var firstNull = nullOrdering == NullOrdering.NullsCompareLower ? 1 : Rows.Count(r => r.S is not null) + 1;
        var cursor = order(source).ToCursorPage(firstNull).EndCursor;

        Assert.Equal(0, CountInclusive(PredicateOf(order, cursor, forward: true)));
        Assert.Equal(0, CountInclusive(PredicateOf(order, cursor, forward: false)));
    }

    [Fact]
    public void BooleanLeadingKey_HasNoBound()
    {
        Func<IQueryable<Row>, IOrderedCursorQuery<Row>> order = s => s.AsCursorQuery().OrderBy(r => r.B).ThenBy(r => r.Id);
        var cursor = order(Rows.AsQueryable()).ToCursorPage(5).EndCursor;

        Assert.Equal(0, CountInclusive(PredicateOf(order, cursor, forward: true)));
    }

    [Theory]
    [InlineData(NullOrdering.NullsCompareLower, false)]
    [InlineData(NullOrdering.NullsCompareHigher, true)]
    public void NullableLeadingKey_BoundCarriesTheSameNullGuardAsTheStrictComparison(NullOrdering nullOrdering, bool admitsNulls)
    {
        Func<IQueryable<Row>, IOrderedCursorQuery<Row>> order =
            s => s.AsCursorQuery(Options(nullOrdering)).OrderBy(r => r.S).ThenBy(r => r.Id);
        var source = NullOrderedSource.Create(Rows, nullOrdering);
        var cursor = order(source)
            .ToCursorPage(nullOrdering == NullOrdering.NullsCompareLower ? Rows.Count(r => r.S is null) + 1 : 1)
            .EndCursor;

        var bound = ((BinaryExpression)PredicateOf(order, cursor, forward: true).Body).Left.ToString();

        Assert.Equal(admitsNulls, bound.Contains("== null", StringComparison.Ordinal));
        Assert.Contains(">=", bound);
    }

    private readonly struct PartialGrade : IComparable<PartialGrade>
    {
        public int Value { get; init; }
        public static bool operator >(PartialGrade a, PartialGrade b) => a.Value > b.Value;
        public static bool operator <(PartialGrade a, PartialGrade b) => a.Value < b.Value;
        public static bool operator ==(PartialGrade a, PartialGrade b) => a.Value == b.Value;
        public static bool operator !=(PartialGrade a, PartialGrade b) => a.Value != b.Value;
        public override bool Equals(object? obj) => obj is PartialGrade grade && grade.Value == Value;
        public override int GetHashCode() => Value;
        public int CompareTo(PartialGrade other) => Value.CompareTo(other.Value);
    }

    private readonly struct FullGrade : IComparable<FullGrade>
    {
        public int Value { get; init; }
        public static bool operator >(FullGrade a, FullGrade b) => a.Value > b.Value;
        public static bool operator <(FullGrade a, FullGrade b) => a.Value < b.Value;
        public static bool operator >=(FullGrade a, FullGrade b) => a.Value >= b.Value;
        public static bool operator <=(FullGrade a, FullGrade b) => a.Value <= b.Value;
        public static bool operator ==(FullGrade a, FullGrade b) => a.Value == b.Value;
        public static bool operator !=(FullGrade a, FullGrade b) => a.Value != b.Value;
        public override bool Equals(object? obj) => obj is FullGrade grade && grade.Value == Value;
        public override int GetHashCode() => Value;
        public int CompareTo(FullGrade other) => Value.CompareTo(other.Value);
    }

    private sealed record PartialRow(int Id, PartialGrade G);

    private sealed record FullRow(int Id, FullGrade G);

    [Fact]
    public void CustomLeadingKeyWithoutInclusiveOperators_KeepsTheStrictPredicate_AndTraversesBothWays()
    {
        var rows = Enumerable.Range(1, 12).Select(i => new PartialRow(i, new PartialGrade { Value = i % 3 })).ToArray();
        Func<IQueryable<PartialRow>, IOrderedCursorQuery<PartialRow>> order =
            s => s.AsCursorQuery().OrderBy(r => r.G).ThenBy(r => r.Id);
        var cursor = order(rows.AsQueryable()).ToCursorPage(5).EndCursor;

        Assert.Equal(0, CountInclusive(PredicateOf(rows, order, cursor, forward: true)));
        Assert.Equal(0, CountInclusive(PredicateOf(rows, order, cursor, forward: false)));
        AssertTraversal(rows, order, rows.OrderBy(r => r.G).ThenBy(r => r.Id).ToList(), r => r.Id);
    }

    [Fact]
    public void CustomLeadingKeyWithInclusiveOperators_GetsTheBound_AndTraversesBothWays()
    {
        var rows = Enumerable.Range(1, 12).Select(i => new FullRow(i, new FullGrade { Value = i % 3 })).ToArray();
        Func<IQueryable<FullRow>, IOrderedCursorQuery<FullRow>> order =
            s => s.AsCursorQuery().OrderBy(r => r.G).ThenBy(r => r.Id);
        var cursor = order(rows.AsQueryable()).ToCursorPage(5).EndCursor;

        Assert.Equal(1, CountInclusive(PredicateOf(rows, order, cursor, forward: true)));
        Assert.Equal(1, CountInclusive(PredicateOf(rows, order, cursor, forward: false)));
        AssertTraversal(rows, order, rows.OrderBy(r => r.G).ThenBy(r => r.Id).ToList(), r => r.Id);
    }

    private static void AssertTraversal<T>(
        T[] items, Func<IQueryable<T>, IOrderedCursorQuery<T>> order, List<T> expectedOrder, Func<T, int> id)
    {
        var source = items.AsQueryable();
        for (var i = 0; i < expectedOrder.Count; i++)
        {
            var cursor = order(source).ToCursorPage(i + 1).EndCursor!;

            var after = order(source).After(cursor).ToCursorPage(3);
            Assert.Equal(expectedOrder.Skip(i + 1).Take(3).Select(id), after.Items.Select(id));

            var before = order(source).Before(cursor).ToCursorPage(3);
            Assert.Equal(expectedOrder.Take(i).TakeLast(3).Select(id), before.Items.Select(id));
        }
    }

    private static Expression<Func<Row, bool>> PredicateOf(
        Func<IQueryable<Row>, IOrderedCursorQuery<Row>> order, string? cursor, bool forward)
    {
        return PredicateOf(Rows, order, cursor, forward);
    }

    private static Expression<Func<T, bool>> PredicateOf<T>(
        IEnumerable<T> items, Func<IQueryable<T>, IOrderedCursorQuery<T>> order, string? cursor, bool forward)
    {
        var provider = new RecordingProvider(items.AsQueryable());
        var query = order(new RecordingQuery<T>(provider));
        _ = (forward ? query.After(cursor) : query.Before(cursor)).ToCursorPage(10);

        var finder = new WhereFinder<T>();
        finder.Visit(provider.Executed);

        return finder.Predicate ?? throw new InvalidOperationException("The executed query has no Where over the entities.");
    }

    private static int CountInclusive(Expression expression)
    {
        var counter = new InclusiveCounter();
        counter.Visit(expression);

        return counter.Count;
    }

    private sealed class WhereFinder<T> : ExpressionVisitor
    {
        public Expression<Func<T, bool>>? Predicate { get; private set; }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == nameof(Queryable.Where) &&
                node.Arguments[1] is UnaryExpression { Operand: Expression<Func<T, bool>> lambda })
            {
                Predicate = lambda;
            }

            return base.VisitMethodCall(node);
        }
    }

    private sealed class InclusiveCounter : ExpressionVisitor
    {
        public int Count { get; private set; }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType is ExpressionType.GreaterThanOrEqual or ExpressionType.LessThanOrEqual)
            {
                Count++;
            }

            return base.VisitBinary(node);
        }
    }

    private sealed class RecordingProvider(IQueryable root) : IQueryProvider
    {
        public Expression Executed { get; private set; } = Expression.Empty();

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type.GetGenericArguments()[0];
            var queryType = typeof(RecordingQuery<>).MakeGenericType(elementType);

            return (IQueryable)Activator.CreateInstance(queryType, this, expression)!;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new RecordingQuery<TElement>(this, expression);
        }

        public object? Execute(Expression expression)
        {
            Executed = expression;
            return root.Provider.Execute(Rewrite(expression));
        }

        public TResult Execute<TResult>(Expression expression)
        {
            Executed = expression;
            return root.Provider.Execute<TResult>(Rewrite(expression));
        }

        public IEnumerator<TElement> Enumerate<TElement>(Expression expression)
        {
            Executed = expression;
            return root.Provider.CreateQuery<TElement>(Rewrite(expression)).GetEnumerator();
        }

        private Expression Rewrite(Expression expression) => new Rewriter(this, root.Expression).Visit(expression);
    }

    private sealed class RecordingQuery<T> : IOrderedQueryable<T>
    {
        private readonly RecordingProvider _provider;

        public RecordingQuery(RecordingProvider provider, Expression? expression = null)
        {
            _provider = provider;
            Expression = expression ?? Expression.Constant(this);
        }

        public Type ElementType => typeof(T);

        public Expression Expression { get; }

        public IQueryProvider Provider => _provider;

        public IEnumerator<T> GetEnumerator() => _provider.Enumerate<T>(Expression);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class Rewriter(RecordingProvider provider, Expression rootExpression) : ExpressionVisitor
    {
        protected override Expression VisitConstant(ConstantExpression node)
        {
            return node.Value is IQueryable query && ReferenceEquals(query.Provider, provider)
                ? rootExpression
                : base.VisitConstant(node);
        }
    }


    private sealed record Spec(
        Func<IQueryable<Row>, CursorQueryOptions, IOrderedCursorQuery<Row>> Cursor,
        Func<IEnumerable<Row>, NullOrdering, IEnumerable<Row>> Oracle);

    private static IComparer<TKey> Cmp<TKey>(NullOrdering nullOrdering) => NullOrderedSource.Comparer<TKey>(nullOrdering);

    private static readonly Dictionary<string, Spec> Specs = new()
    {
        ["D asc, Id"] = new(
            (s, o) => s.AsCursorQuery(o).OrderBy(r => r.D).ThenBy(r => r.Id),
            (r, n) => r.OrderBy(x => x.D).ThenBy(x => x.Id)),
        ["M desc, Id desc"] = new(
            (s, o) => s.AsCursorQuery(o).OrderByDescending(r => r.M).ThenByDescending(r => r.Id),
            (r, n) => r.OrderByDescending(x => x.M).ThenByDescending(x => x.Id)),
        ["K asc, Id"] = new(
            (s, o) => s.AsCursorQuery(o).OrderBy(r => r.K).ThenBy(r => r.Id),
            (r, n) => r.OrderBy(x => x.K).ThenBy(x => x.Id)),
        ["G asc, Id desc"] = new(
            (s, o) => s.AsCursorQuery(o).OrderBy(r => r.G).ThenByDescending(r => r.Id),
            (r, n) => r.OrderBy(x => x.G).ThenByDescending(x => x.Id)),
        ["B asc, Id"] = new(
            (s, o) => s.AsCursorQuery(o).OrderBy(r => r.B).ThenBy(r => r.Id),
            (r, n) => r.OrderBy(x => x.B).ThenBy(x => x.Id)),
        ["S asc, Id"] = new(
            (s, o) => s.AsCursorQuery(o).OrderBy(r => r.S).ThenBy(r => r.Id),
            (r, n) => r.OrderBy(x => x.S, Cmp<string?>(n)).ThenBy(x => x.Id)),
        ["S desc, Id desc"] = new(
            (s, o) => s.AsCursorQuery(o).OrderByDescending(r => r.S).ThenByDescending(r => r.Id),
            (r, n) => r.OrderByDescending(x => x.S, Cmp<string?>(n)).ThenByDescending(x => x.Id)),
        ["N asc, S desc, Id"] = new(
            (s, o) => s.AsCursorQuery(o).OrderBy(r => r.N).ThenByDescending(r => r.S).ThenBy(r => r.Id),
            (r, n) => r.OrderBy(x => x.N, Cmp<int?>(n)).ThenByDescending(x => x.S, Cmp<string?>(n)).ThenBy(x => x.Id)),
        ["N desc, D desc, S, Id desc"] = new(
            (s, o) => s.AsCursorQuery(o).OrderByDescending(r => r.N).ThenByDescending(r => r.D).ThenBy(r => r.S).ThenByDescending(r => r.Id),
            (r, n) => r.OrderByDescending(x => x.N, Cmp<int?>(n)).ThenByDescending(x => x.D).ThenBy(x => x.S, Cmp<string?>(n)).ThenByDescending(x => x.Id))
    };

    public static TheoryData<string, NullOrdering, int> Cases()
    {
        var data = new TheoryData<string, NullOrdering, int>();
        foreach (var spec in Specs.Keys)
        foreach (var nullOrdering in Enum.GetValues<NullOrdering>())
        foreach (var pageSize in new[] { 1, 4 })
        {
            data.Add(spec, nullOrdering, pageSize);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void EveryRowAsCursor_ReturnsTheOraclePage_InBothDirections(string name, NullOrdering nullOrdering, int pageSize)
    {
        var spec = Specs[name];
        var source = NullOrderedSource.Create(Rows, nullOrdering);
        var options = Options(nullOrdering);
        var all = spec.Oracle(Rows, nullOrdering).ToList();

        for (var i = 0; i < all.Count; i++)
        {
            var cursor = spec.Cursor(source, options).ToCursorPage(i + 1).EndCursor!;

            var after = spec.Cursor(source, options).After(cursor).ToCursorPage(pageSize);
            Assert.Equal(all.Skip(i + 1).Take(pageSize).Select(r => r.Id), after.Items.Select(r => r.Id));

            var before = spec.Cursor(source, options).Before(cursor).ToCursorPage(pageSize);
            Assert.Equal(all.Take(i).TakeLast(pageSize).Select(r => r.Id), before.Items.Select(r => r.Id));
        }
    }
}
