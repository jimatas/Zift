namespace Zift.Pagination.Cursor;

using Fixture;
using Ordering;

public sealed class OrderingClauseTests
{
    [Fact]
    public void Create_WithInt32Key_ReturnsTypedOrderingClause()
    {
        var selector = (Expression<Func<TestClass, int>>)(e => e.Int32Value);

        var clause = OrderingClause<TestClass>.Create(
            selector,
            OrderingDirection.Ascending);

        Assert.IsType<OrderingClause<TestClass, int>>(clause);
    }

    [Fact]
    public void Create_WithNullableKey_ReturnsTypedOrderingClause()
    {
        var selector = (Expression<Func<TestClass, int?>>)(e => e.NullableInt32Value);

        var clause = OrderingClause<TestClass>.Create(
            selector,
            OrderingDirection.Ascending);

        Assert.IsType<OrderingClause<TestClass, int?>>(clause);
    }

    [Fact]
    public void Create_WithStringKey_ReturnsTypedOrderingClause()
    {
        var selector = (Expression<Func<TestClass, string?>>)(e => e.StringValue);

        var clause = OrderingClause<TestClass>.Create(
            selector,
            OrderingDirection.Descending);

        Assert.IsType<OrderingClause<TestClass, string?>>(clause);
    }

    [Fact]
    public void Create_WithGuidKey_ReturnsTypedOrderingClause()
    {
        var selector = (Expression<Func<TestClass, Guid>>)(e => e.GuidValue);

        var clause = OrderingClause<TestClass>.Create(
            selector,
            OrderingDirection.Ascending);

        Assert.IsType<OrderingClause<TestClass, Guid>>(clause);
    }

    [Fact]
    public void Create_WithDateTimeKey_ReturnsTypedOrderingClause()
    {
        var selector = (Expression<Func<TestClass, DateTime>>)(e => e.DateTimeValue);

        var clause = OrderingClause<TestClass>.Create(
            selector,
            OrderingDirection.Ascending);

        Assert.IsType<OrderingClause<TestClass, DateTime>>(clause);
    }

    [Fact]
    public void Create_SameKeyType_ReusesFactory()
    {
        var selector1 = (Expression<Func<TestSubclass, int>>)(e => e.Int32Value);
        var selector2 = (Expression<Func<TestSubclass, int>>)(e => e.OtherInt32Value);

        var clause1 = OrderingClause<TestSubclass>.Create(selector1, OrderingDirection.Ascending);
        var clause2 = OrderingClause<TestSubclass>.Create(selector2, OrderingDirection.Descending);

        Assert.IsType<OrderingClause<TestSubclass, int>>(clause1);
        Assert.IsType<OrderingClause<TestSubclass, int>>(clause2);
    }

    [Fact]
    public void Reverse_PreservesTypedClause()
    {
        var selector = (Expression<Func<TestClass, int>>)(e => e.Int32Value);

        var clause = OrderingClause<TestClass>.Create(
            selector,
            OrderingDirection.Ascending);

        var reversed = clause.Reverse();

        Assert.IsType<OrderingClause<TestClass, int>>(reversed);
        Assert.Equal(OrderingDirection.Descending, reversed.Direction);
    }

    [Fact]
    public void ApplyTo_OnQueryable_AppliesOrdering()
    {
        var data = new[]
        {
            new TestClass { Int32Value = 2 },
            new TestClass { Int32Value = 3 },
            new TestClass { Int32Value = 1 }
        }.AsQueryable();

        var clause = OrderingClause<TestClass>.Create(
            (Expression<Func<TestClass, int>>)(e => e.Int32Value),
            OrderingDirection.Ascending);

        var ordered = clause.ApplyTo(data).ToList();

        Assert.Equal([1, 2, 3], ordered.Select(e => e.Int32Value));
    }

    [Fact]
    public void ApplyTo_OnOrderedQueryable_AppliesSecondaryOrdering()
    {
        var data = new[]
        {
            new TestClass { Int32Value = 2, StringValue = "C" },
            new TestClass { Int32Value = 2, StringValue = "B" },
            new TestClass { Int32Value = 1, StringValue = "A" }
        }.AsQueryable();

        var primary = OrderingClause<TestClass>.Create(
            (Expression<Func<TestClass, int>>)(e => e.Int32Value),
            OrderingDirection.Ascending);

        var secondary = OrderingClause<TestClass>.Create(
            (Expression<Func<TestClass, string?>>)(e => e.StringValue),
            OrderingDirection.Ascending);

        var ordered = secondary
            .ApplyTo(primary.ApplyTo(data))
            .ToList();

        Assert.Equal(
            [
                (1, "A"),
                (2, "B"),
                (2, "C")
            ],
            ordered.Select(e => (e.Int32Value, e.StringValue!)));
    }

    private class TestSubclass : TestClass
    {
        public int OtherInt32Value { get; set; }
    }
}
