namespace Zift.Pagination.Cursor;

using Fixture;

public sealed class CursorPaginationBehaviorTests
{
    [Fact]
    public void ToCursorPage_FirstPageForward_HasNextOnly_WithNextCursorAtLastItem()
    {
        var source = Enumerable.Range(1, 10)
            .Select(i => new TestClass { Int32Value = i })
            .AsQueryable();

        var page = source
            .AsCursorQuery()
            .OrderBy(e => e.Int32Value)
            .ToCursorPage(pageSize: 3);

        Assert.Equal([1, 2, 3], page.Items.Select(i => i.Int32Value).ToArray());

        Assert.True(page.HasNext);
        Assert.False(page.HasPrevious);

        Assert.NotNull(page.NextCursor);
        Assert.Null(page.PreviousCursor);

        var decoded = CursorValues.Decode(page.NextCursor!, [typeof(int)]);
        Assert.Equal(3, (int)decoded.Values[0]!);
    }

    [Fact]
    public void ToCursorPage_SecondPageAfter_HasNextAndPrevious_WithCursorsAtBounds()
    {
        var source = Enumerable.Range(1, 10)
            .Select(i => new TestClass { Int32Value = i })
            .AsQueryable();

        var first = source
            .AsCursorQuery()
            .OrderBy(e => e.Int32Value)
            .ToCursorPage(pageSize: 3);

        var after = first.NextCursor;

        var second = source
            .AsCursorQuery()
            .OrderBy(e => e.Int32Value)
            .After(after!)
            .ToCursorPage(pageSize: 3);

        Assert.Equal([4, 5, 6], second.Items.Select(i => i.Int32Value).ToArray());

        Assert.True(second.HasNext);
        Assert.True(second.HasPrevious);

        var decoded = CursorValues.Decode(second.NextCursor!, [typeof(int)]);
        Assert.Equal(6, (int)decoded.Values[0]!);

        decoded = CursorValues.Decode(second.PreviousCursor!, [typeof(int)]);
        Assert.Equal(4, (int)decoded.Values[0]!);
    }

    [Fact]
    public void ToCursorPage_LastPageAfter_HasPreviousOnly_WithoutNextCursor()
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

        Assert.Equal([8, 9, 10], page.Items.Select(i => i.Int32Value).ToArray());

        Assert.False(page.HasNext);
        Assert.True(page.HasPrevious);

        Assert.Null(page.NextCursor);

        var decoded = CursorValues.Decode(page.PreviousCursor!, [typeof(int)]);
        Assert.Equal(8, (int)decoded.Values[0]!);
    }

    [Fact]
    public void ToCursorPage_BeforeCursor_ReturnsPreviousItems_WithBothCursors()
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

        Assert.Equal([5, 6, 7], page.Items.Select(i => i.Int32Value).ToArray());

        Assert.True(page.HasNext);
        Assert.True(page.HasPrevious);

        var decoded = CursorValues.Decode(page.NextCursor!, [typeof(int)]);
        Assert.Equal(7, (int)decoded.Values[0]!);

        decoded = CursorValues.Decode(page.PreviousCursor!, [typeof(int)]);
        Assert.Equal(5, (int)decoded.Values[0]!);
    }

    [Fact]
    public void ToCursorPage_BeforeFirstItem_ReturnsEmptyPage_WithoutCursors()
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
        Assert.False(page.HasNext);
        Assert.False(page.HasPrevious);
        Assert.Null(page.NextCursor);
        Assert.Null(page.PreviousCursor);
    }
}
