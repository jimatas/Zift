namespace Zift.Pagination.Cursor;

using Fixture;
using Ordering;

public sealed class CursorQueryOrderingTests
{
    [Fact]
    public void AsCursorQuery_NullSource_ThrowsArgumentNullException()
    {
        IQueryable<TestClass> source = null!;

        Assert.Throws<ArgumentNullException>(() => source.AsCursorQuery());
    }

    [Fact]
    public void OrderBy_WithExpression_AddsSingleAscendingClause()
    {
        var source = new[]
        {
            new TestClass { Int32Value = 1 },
            new TestClass { Int32Value = 2 }
        }.AsQueryable();

        var ordered = source
            .AsCursorQuery()
            .OrderBy(e => e.Int32Value);

        var orderedState = Assert.IsType<ICursorQueryState<TestClass>>(ordered, exactMatch: false);

        Assert.Same(source, orderedState.Source);

        var clause = Assert.Single(orderedState.State.Ordering.Clauses);
        Assert.Equal(OrderingDirection.Ascending, clause.Direction);
        Assert.Equal(typeof(int), clause.KeySelector.ReturnType);
    }

    [Fact]
    public void OrderByDescending_WithExpression_AddsSingleDescendingClause()
    {
        var source = new[]
        {
            new TestClass { Int32Value = 1 },
            new TestClass { Int32Value = 2 }
        }.AsQueryable();

        var ordered = source
            .AsCursorQuery()
            .OrderByDescending(e => e.Int32Value);

        var (state, src) = GetState<TestClass>(ordered);

        Assert.Same(source, src);

        var clause = Assert.Single(state.Ordering.Clauses);
        Assert.Equal(OrderingDirection.Descending, clause.Direction);
        Assert.Equal(typeof(int), clause.KeySelector.ReturnType);
    }

    [Fact]
    public void OrderBy_WithStringClause_AddsSingleDescendingClause()
    {
        var source = new[]
        {
            new TestClass { Int32Value = 1 },
            new TestClass { Int32Value = 2 }
        }.AsQueryable();

        var ordered = source
            .AsCursorQuery()
            .OrderBy("Int32Value desc");

        var (state, _) = GetState<TestClass>(ordered);

        var clause = Assert.Single(state.Ordering.Clauses);
        Assert.Equal(OrderingDirection.Descending, clause.Direction);
        Assert.Equal(typeof(int), clause.KeySelector.ReturnType);
    }

    [Fact]
    public void OrderBy_EmptyString_ThrowsInvalidOperationException()
    {
        var source = Array.Empty<TestClass>().AsQueryable();
        var query = source.AsCursorQuery();

        var ex = Assert.Throws<InvalidOperationException>(() => query.OrderBy(string.Empty));

        Assert.Contains("Order-by expression must contain at least one clause", ex.Message);
    }

    [Fact]
    public void ThenBy_WithExistingOrdering_AppendsClauseAndPreservesSource()
    {
        var source = new[]
        {
            new TestClass { Int32Value = 1, StringValue = "B" },
            new TestClass { Int32Value = 1, StringValue = "A" }
        }.AsQueryable();

        var ordered = source
            .AsCursorQuery()
            .OrderBy(e => e.Int32Value);

        var chained = ordered.ThenBy(e => e.StringValue);
        var (state, src) = GetState<TestClass>(chained);

        Assert.Same(source, src);
        Assert.Equal(2, state.Ordering.Clauses.Count);
        
        Assert.Equal(OrderingDirection.Ascending, state.Ordering.Clauses[0].Direction);
        Assert.Equal(typeof(int), state.Ordering.Clauses[0].KeySelector.ReturnType);

        Assert.Equal(OrderingDirection.Ascending, state.Ordering.Clauses[1].Direction);
        Assert.Equal(typeof(string), state.Ordering.Clauses[1].KeySelector.ReturnType);
    }

    [Fact]
    public void ThenByDescending_WithExistingOrdering_AppendsClauseAndPreservesSource()
    {
        var source = new[]
        {
            new TestClass { Int32Value = 1, StringValue = "B" },
            new TestClass { Int32Value = 1, StringValue = "A" }
        }.AsQueryable();

        var ordered = source
            .AsCursorQuery()
            .OrderBy(e => e.Int32Value);

        var chained = ordered.ThenByDescending(e => e.StringValue);
        var (state, src) = GetState<TestClass>(chained);

        Assert.Same(source, src);
        Assert.Equal(2, state.Ordering.Clauses.Count);

        Assert.Equal(OrderingDirection.Ascending, state.Ordering.Clauses[0].Direction);
        Assert.Equal(typeof(int), state.Ordering.Clauses[0].KeySelector.ReturnType);

        Assert.Equal(OrderingDirection.Descending, state.Ordering.Clauses[1].Direction);
        Assert.Equal(typeof(string), state.Ordering.Clauses[1].KeySelector.ReturnType);
    }

    private static (CursorQueryState<T> State, IQueryable<T> Source) GetState<T>(object query)
    {
        var state = Assert.IsType<ICursorQueryState<T>>(query, exactMatch: false);

        return (state.State, state.Source);
    }
}
