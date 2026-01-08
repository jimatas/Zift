namespace Zift.Pagination.Cursor;

using Fixture;

public sealed class CursorPaginationBehaviorTests
{
    [Fact]
    public void FirstPage_ReturnsFirstItems_WithAnchors_AndNextPageOnly()
    {
        var source = Enumerable.Range(1, 10)
            .Select(i => new TestClass { Int32Value = i })
            .AsQueryable();

        var page = source
            .AsCursorQuery()
            .OrderBy(e => e.Int32Value)
            .ToCursorPage(pageSize: 3);

        Assert.Equal([1, 2, 3], page.Items.Select(i => i.Int32Value));

        Assert.True(page.HasNextPage);
        Assert.False(page.HasPreviousPage);

        Assert.NotNull(page.StartCursor);
        Assert.NotNull(page.EndCursor);

        var start = CursorValues.Decode(page.StartCursor!, [typeof(int)]);
        var end = CursorValues.Decode(page.EndCursor!, [typeof(int)]);

        Assert.Equal(1, (int)start.Values[0]!);
        Assert.Equal(3, (int)end.Values[0]!);
    }

    [Fact]
    public void AfterEndCursor_ReturnsNextItems_WithAnchors_AndBothDirectionsAvailable()
    {
        var source = Enumerable.Range(1, 10)
            .Select(i => new TestClass { Int32Value = i })
            .AsQueryable();

        var first = source
            .AsCursorQuery()
            .OrderBy(e => e.Int32Value)
            .ToCursorPage(pageSize: 3);

        var second = source
            .AsCursorQuery()
            .OrderBy(e => e.Int32Value)
            .After(first.EndCursor)
            .ToCursorPage(pageSize: 3);

        Assert.Equal([4, 5, 6], second.Items.Select(i => i.Int32Value));

        Assert.True(second.HasNextPage);
        Assert.True(second.HasPreviousPage);

        Assert.NotNull(second.StartCursor);
        Assert.NotNull(second.EndCursor);

        var start = CursorValues.Decode(second.StartCursor!, [typeof(int)]);
        var end = CursorValues.Decode(second.EndCursor!, [typeof(int)]);

        Assert.Equal(4, (int)start.Values[0]!);
        Assert.Equal(6, (int)end.Values[0]!);
    }

    [Fact]
    public void AfterEndCursor_OnLastPage_ReturnsFinalItems_WithPreviousPageOnly()
    {
        var source = Enumerable.Range(1, 10)
            .Select(i => new TestClass { Int32Value = i })
            .AsQueryable();

        var cursor = new CursorValues([7]).Encode();

        var page = source
            .AsCursorQuery()
            .OrderBy(e => e.Int32Value)
            .After(cursor)
            .ToCursorPage(pageSize: 3);

        Assert.Equal([8, 9, 10], page.Items.Select(i => i.Int32Value));

        Assert.False(page.HasNextPage);
        Assert.True(page.HasPreviousPage);

        Assert.NotNull(page.StartCursor);
        Assert.NotNull(page.EndCursor);

        var start = CursorValues.Decode(page.StartCursor!, [typeof(int)]);
        var end = CursorValues.Decode(page.EndCursor!, [typeof(int)]);

        Assert.Equal(8, (int)start.Values[0]!);
        Assert.Equal(10, (int)end.Values[0]!);
    }

    [Fact]
    public void BeforeStartCursor_ReturnsPreviousItems_WithAnchors_AndBothDirectionsAvailable()
    {
        var source = Enumerable.Range(1, 10)
            .Select(i => new TestClass { Int32Value = i })
            .AsQueryable();

        var cursor = new CursorValues([8]).Encode();

        var page = source
            .AsCursorQuery()
            .OrderBy(e => e.Int32Value)
            .Before(cursor)
            .ToCursorPage(pageSize: 3);

        Assert.Equal([5, 6, 7], page.Items.Select(i => i.Int32Value));

        Assert.True(page.HasNextPage);
        Assert.True(page.HasPreviousPage);

        Assert.NotNull(page.StartCursor);
        Assert.NotNull(page.EndCursor);

        var start = CursorValues.Decode(page.StartCursor!, [typeof(int)]);
        var end = CursorValues.Decode(page.EndCursor!, [typeof(int)]);

        Assert.Equal(5, (int)start.Values[0]!);
        Assert.Equal(7, (int)end.Values[0]!);
    }

    [Fact]
    public void BeforeFirstItem_ReturnsEmptyPage_WithoutAnchors_AndNoNavigation()
    {
        var source = Enumerable.Range(1, 10)
            .Select(i => new TestClass { Int32Value = i })
            .AsQueryable();

        var cursor = new CursorValues([1]).Encode();

        var page = source
            .AsCursorQuery()
            .OrderBy(e => e.Int32Value)
            .Before(cursor)
            .ToCursorPage(pageSize: 3);

        Assert.Empty(page.Items);

        Assert.False(page.HasNextPage);
        Assert.False(page.HasPreviousPage);

        Assert.Null(page.StartCursor);
        Assert.Null(page.EndCursor);
    }

    [Fact]
    public void CompositeOrdering_AnchorsEncodeAllOrderingKeys()
    {
        var source = new[]
        {
            new TestClass { Int32Value = 1, StringValue = "A" },
            new TestClass { Int32Value = 1, StringValue = "B" },
            new TestClass { Int32Value = 2, StringValue = "A" },
            new TestClass { Int32Value = 2, StringValue = "B" }
        }.AsQueryable();

        var firstPage = source
            .AsCursorQuery()
            .OrderBy(e => e.Int32Value)
            .ThenBy(e => e.StringValue)
            .ToCursorPage(pageSize: 2);

        Assert.NotNull(firstPage.StartCursor);
        Assert.NotNull(firstPage.EndCursor);

        var start = CursorValues.Decode(
            firstPage.StartCursor!,
            [typeof(int), typeof(string)]);

        var end = CursorValues.Decode(
            firstPage.EndCursor!,
            [typeof(int), typeof(string)]);

        Assert.Equal(1, start.Values[0]);
        Assert.Equal("A", start.Values[1]);

        Assert.Equal(1, end.Values[0]);
        Assert.Equal("B", end.Values[1]);

        var secondPage = source
            .AsCursorQuery()
            .OrderBy(e => e.Int32Value)
            .ThenBy(e => e.StringValue)
            .After(firstPage.EndCursor)
            .ToCursorPage(pageSize: 2);

        Assert.Equal(
            [(2, "A"), (2, "B")],
            secondPage.Items.Select(e => (e.Int32Value, e.StringValue!)));
    }
}
