namespace Zift.Pagination.Cursor;

using Fixture;
using Microsoft.EntityFrameworkCore;

public sealed class CursorPaginationSqliteIntegrationTests
{
    [Fact]
    public async Task ToCursorPage_FirstPageByNameAscending_ReturnsBooksWithNextCursor()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var page = fixture.Context.Categories
            .AsCursorQuery()
            .OrderBy(c => c.Name!)
            .ToCursorPage(pageSize: 1);

        var category = Assert.Single(page.Items);
        Assert.Equal("Books", category.Name);

        Assert.True(page.HasNext);
        Assert.False(page.HasPrevious);
        Assert.NotNull(page.NextCursor);
        Assert.Null(page.PreviousCursor);
    }

    [Fact]
    public async Task ToCursorPage_SecondPageAfter_ReturnsElectronicsWithPreviousCursor()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var firstPage = fixture.Context.Categories
            .AsCursorQuery()
            .OrderBy(c => c.Name!)
            .ToCursorPage(pageSize: 1);

        var after = firstPage.NextCursor;

        var secondPage = fixture.Context.Categories
            .AsCursorQuery()
            .OrderBy(c => c.Name!)
            .After(after!)
            .ToCursorPage(pageSize: 1);

        var category = Assert.Single(secondPage.Items);
        Assert.Equal("Electronics", category.Name);

        Assert.False(secondPage.HasNext);
        Assert.True(secondPage.HasPrevious);
        Assert.Null(secondPage.NextCursor);
        Assert.NotNull(secondPage.PreviousCursor);
    }

    [Fact]
    public async Task ToCursorPageAsync_FirstPageByNameAscending_ReturnsBooksWithNextCursor()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var page = await fixture.Context.Categories
            .AsCursorQuery()
            .OrderBy(c => c.Name!)
            .ToCursorPageAsync(pageSize: 1);

        var category = Assert.Single(page.Items);
        Assert.Equal("Books", category.Name);

        Assert.True(page.HasNext);
        Assert.False(page.HasPrevious);
        Assert.NotNull(page.NextCursor);
        Assert.Null(page.PreviousCursor);
    }

    [Fact]
    public async Task ToCursorPageAsync_SecondPageAfter_ReturnsElectronicsWithPreviousCursor()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var firstPage = await fixture.Context.Categories
            .AsCursorQuery()
            .OrderBy(c => c.Name!)
            .ToCursorPageAsync(pageSize: 1);

        var after = firstPage.NextCursor;

        var secondPage = await fixture.Context.Categories
            .AsCursorQuery()
            .OrderBy(c => c.Name!)
            .After(after!)
            .ToCursorPageAsync(pageSize: 1);

        var category = Assert.Single(secondPage.Items);
        Assert.Equal("Electronics", category.Name);

        Assert.False(secondPage.HasNext);
        Assert.True(secondPage.HasPrevious);
        Assert.Null(secondPage.NextCursor);
        Assert.NotNull(secondPage.PreviousCursor);
    }
}
