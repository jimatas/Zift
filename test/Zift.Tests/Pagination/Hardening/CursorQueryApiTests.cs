using Zift.Pagination.Cursor;

namespace Zift.Pagination.Tests;

public class CursorQueryApiTests
{
    private record Item(int Id, string Name);

    private static IQueryable<Item> Items => new[] { new Item(1, "A"), new Item(2, "B") }.AsQueryable();

    [Fact]
    public void AsCursorQuery_IsNotExecutable_UntilOrdered()
    {
        object query = Items.AsCursorQuery();

        Assert.False(query is IExecutableCursorQuery<Item> && query is not IOrderedCursorQuery<Item>);
    }

    [Fact]
    public void After_WithNullCursor_IsAccepted_AsFirstPage()
    {
        var query = Items.AsCursorQuery().OrderBy(i => i.Id).After(null);

        Assert.NotNull(query);
    }

    [Fact]
    public void After_WithBlankCursor_Throws()
    {
        var ordered = Items.AsCursorQuery().OrderBy(i => i.Id);

        Assert.Throws<ArgumentException>(() => ordered.After(" "));
    }

    [Fact]
    public void CursorPage_Map_PreservesCursorsAndFlags()
    {
        var page = new CursorPage<int>([1, 2], "s", "e", hasNextPage: true, hasPreviousPage: false)
            .Map(n => n.ToString());

        Assert.Equal(["1", "2"], page.Items);
        Assert.Equal("e", page.EndCursor);
        Assert.True(page.HasNextPage);
    }

    [Fact]
    public void CursorPage_Empty_HasNoItemsCursorsOrContinuation()
    {
        var page = CursorPage<int>.Empty;

        Assert.Empty(page.Items);
        Assert.Null(page.StartCursor);
        Assert.Null(page.EndCursor);
        Assert.False(page.HasNextPage);
        Assert.False(page.HasPreviousPage);
    }

    [Fact]
    public void CursorPage_NextPageRequiresEndCursor()
    {
        var error = Assert.Throws<ArgumentException>(() => new CursorPage<int>([1], "s", null, true, false));
        Assert.Equal("endCursor", error.ParamName);
    }

    [Fact]
    public void CursorPage_PreviousPageRequiresStartCursor()
    {
        var error = Assert.Throws<ArgumentException>(() => new CursorPage<int>([1], null, "e", false, true));
        Assert.Equal("startCursor", error.ParamName);
    }

    [Fact]
    public void CursorPage_WithoutContinuation_AllowsNullCursors()
    {
        var page = new CursorPage<int>([], null, null, false, false);
        Assert.Null(page.StartCursor);
        Assert.Null(page.EndCursor);
    }

    [Fact]
    public void CursorPage_WithContinuation_AcceptsRequiredCursors()
    {
        var page = new CursorPage<int>([1], "s", "e", true, true);
        Assert.True(page.HasNextPage);
        Assert.True(page.HasPreviousPage);
    }

    [Fact]
    public void CursorPage_NonEmptyItems_RequireStartCursor()
    {
        var error = Assert.Throws<ArgumentNullException>(() => new CursorPage<int>([1], null, "e", false, false));
        Assert.Equal("startCursor", error.ParamName);
    }

    [Fact]
    public void CursorPage_NonEmptyItems_RequireEndCursor()
    {
        var error = Assert.Throws<ArgumentNullException>(() => new CursorPage<int>([1], "s", null, false, false));
        Assert.Equal("endCursor", error.ParamName);
    }

    [Theory]
    [InlineData("", "e", "startCursor")]
    [InlineData(" ", "e", "startCursor")]
    [InlineData("s", "", "endCursor")]
    [InlineData("s", " ", "endCursor")]
    public void CursorPage_BlankCursors_AreRejected(string startCursor, string endCursor, string paramName)
    {
        var error = Assert.Throws<ArgumentException>(() => new CursorPage<int>([1], startCursor, endCursor, false, false));
        Assert.Equal(paramName, error.ParamName);
    }

    [Fact]
    public void CursorPage_SingleItem_AllowsSameStartAndEndCursor()
    {
        var page = new CursorPage<int>([1], "c", "c", false, false);
        Assert.Equal("c", page.StartCursor);
        Assert.Equal("c", page.EndCursor);
    }

    [Theory]
    [InlineData("s", null, false, false)]
    [InlineData(null, "e", false, false)]
    [InlineData(null, null, true, false)]
    [InlineData(null, null, false, true)]
    [InlineData("s", "e", true, true)]
    public void CursorPage_EmptyItems_RejectCursorsAndContinuation(
        string? startCursor, string? endCursor, bool hasNextPage, bool hasPreviousPage)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => new CursorPage<int>([], startCursor, endCursor, hasNextPage, hasPreviousPage));
    }

    [Fact]
    public void CursorPage_Map_OfEmptyPage_StaysEmpty()
    {
        var page = new CursorPage<int>([], null, null, false, false).Map(n => n.ToString());

        Assert.Empty(page.Items);
        Assert.Null(page.StartCursor);
    }
}
