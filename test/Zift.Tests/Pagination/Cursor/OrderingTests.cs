namespace Zift.Pagination.Cursor;

using Fixture;
using Ordering;

public sealed class OrderingTests
{
    [Fact]
    public void Reverse_WhenEmpty_ReturnsSameInstance()
    {
        var ordering = Ordering<TestClass>.Empty;

        var reversed = ordering.Reverse();

        Assert.Same(ordering, reversed);
        Assert.True(reversed.IsEmpty);
    }

    [Fact]
    public void ApplyTo_WithMultipleClauses_AppliesAllInOrder()
    {
        var data = new[]
        {
            new TestClass { Int32Value = 2, StringValue = "B" },
            new TestClass { Int32Value = 1, StringValue = "C" },
            new TestClass { Int32Value = 1, StringValue = "A" }
        }.AsQueryable();

        var ordering = Ordering<TestClass>.Empty
            .Append(OrderingClause<TestClass>.Create(
                (Expression<Func<TestClass, int>>)(e => e.Int32Value),
                OrderingDirection.Ascending))
            .Append(OrderingClause<TestClass>.Create(
                (Expression<Func<TestClass, string?>>)(e => e.StringValue),
                OrderingDirection.Ascending));

        var ordered = ordering.ApplyTo(data).ToList();

        Assert.Equal(
            [
                (1, "A"),
                (1, "C"),
                (2, "B")
            ],
            ordered.Select(e => (e.Int32Value, e.StringValue!)));
    }

    [Fact]
    public void ApplyTo_WhenEmpty_ThrowsInvalidOperationException()
    {
        var data = Array.Empty<TestClass>().AsQueryable();

        var ex = Assert.Throws<InvalidOperationException>(
            () => Ordering<TestClass>.Empty.ApplyTo(data));

        Assert.Contains("any ordering clauses", ex.Message);
    }
}
