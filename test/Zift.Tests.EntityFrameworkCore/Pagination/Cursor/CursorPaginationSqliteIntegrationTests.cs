namespace Zift.Pagination.Cursor;

using Fixture;
using Microsoft.EntityFrameworkCore;
using Zift.Pagination.Offset;

public sealed class CursorPaginationSqliteIntegrationTests
{
    [Fact]
    public async Task ToCursorPage_FirstPageByNameAscending_ReturnsBooks_WithAnchors_AndNextPageOnly()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var page = fixture.Context.Categories
            .AsCursorQuery()
            .OrderBy(c => c.Name!)
            .ToCursorPage(pageSize: 1);

        var category = Assert.Single(page.Items);
        Assert.Equal("Books", category.Name);

        Assert.True(page.HasNextPage);
        Assert.False(page.HasPreviousPage);
        
        Assert.Equal(page.StartCursor, page.EndCursor);
    }

    [Fact]
    public async Task ToCursorPage_SecondPageAfter_ReturnsElectronics_WithAnchors_AndPreviousPageOnly()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var firstPage = fixture.Context.Categories
            .AsCursorQuery()
            .OrderBy(c => c.Name!)
            .ToCursorPage(pageSize: 1);

        var secondPage = fixture.Context.Categories
            .AsCursorQuery()
            .OrderBy(c => c.Name!)
            .After(firstPage.EndCursor)
            .ToCursorPage(pageSize: 1);

        var category = Assert.Single(secondPage.Items);
        Assert.Equal("Electronics", category.Name);

        Assert.False(secondPage.HasNextPage);
        Assert.True(secondPage.HasPreviousPage);

        Assert.Equal(secondPage.StartCursor, secondPage.EndCursor);
    }

    [Fact]
    public async Task ToCursorPageAsync_FirstPageByNameAscending_ReturnsBooks_WithAnchors_AndNextPageOnly()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var page = await fixture.Context.Categories
            .AsCursorQuery()
            .OrderBy(c => c.Name!)
            .ToCursorPageAsync(pageSize: 1);

        var category = Assert.Single(page.Items);
        Assert.Equal("Books", category.Name);

        Assert.True(page.HasNextPage);
        Assert.False(page.HasPreviousPage);

        Assert.Equal(page.StartCursor, page.EndCursor);
    }

    [Fact]
    public async Task ToCursorPageAsync_SecondPageAfter_ReturnsElectronics_WithAnchors_AndPreviousPageOnly()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var firstPage = await fixture.Context.Categories
            .AsCursorQuery()
            .OrderBy(c => c.Name!)
            .ToCursorPageAsync(pageSize: 1);

        var secondPage = await fixture.Context.Categories
            .AsCursorQuery()
            .OrderBy(c => c.Name!)
            .After(firstPage.EndCursor)
            .ToCursorPageAsync(pageSize: 1);

        var category = Assert.Single(secondPage.Items);
        Assert.Equal("Electronics", category.Name);

        Assert.False(secondPage.HasNextPage);
        Assert.True(secondPage.HasPreviousPage);

        Assert.Equal(secondPage.StartCursor, secondPage.EndCursor);
    }
}
