namespace Zift.Pagination.Offset;

using Fixture;

public sealed class OffsetPaginationSqliteIntegrationTests
{
    [Fact]
    public async Task ToPage_FirstPageByNameAscending_ReturnsBooksWithNextPage()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var page = fixture.Context.Categories
            .OrderBy(c => c.Name!)
            .ToPage(pageNumber: 1, pageSize: 1);

        var category = Assert.Single(page.Items);
        Assert.Equal("Books", category.Name);

        Assert.Equal(1, page.PageNumber);
        Assert.Equal(1, page.PageSize);
        Assert.Equal(2, page.PageCount);

        Assert.True(page.HasNextPage);
        Assert.False(page.HasPreviousPage);
    }

    [Fact]
    public async Task ToPage_SecondPageByNameAscending_ReturnsElectronicsWithPreviousPage()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var page = fixture.Context.Categories
            .OrderBy(c => c.Name!)
            .ToPage(pageNumber: 2, pageSize: 1);

        var category = Assert.Single(page.Items);
        Assert.Equal("Electronics", category.Name);

        Assert.Equal(2, page.PageNumber);
        Assert.Equal(2, page.PageCount);

        Assert.False(page.HasNextPage);
        Assert.True(page.HasPreviousPage);
    }

    [Fact]
    public async Task ToPageAsync_FirstPageByNameAscending_ReturnsBooksWithNextPage()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var page = await fixture.Context.Categories
            .OrderBy(c => c.Name!)
            .ToPageAsync(pageNumber: 1, pageSize: 1);

        var category = Assert.Single(page.Items);
        Assert.Equal("Books", category.Name);

        Assert.Equal(1, page.PageNumber);
        Assert.Equal(1, page.PageSize);
        Assert.Equal(2, page.PageCount);

        Assert.True(page.HasNextPage);
        Assert.False(page.HasPreviousPage);
    }

    [Fact]
    public async Task ToPageAsync_SecondPageByNameAscending_ReturnsElectronicsWithPreviousPage()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var page = await fixture.Context.Categories
            .OrderBy(c => c.Name!)
            .ToPageAsync(pageNumber: 2, pageSize: 1);

        var category = Assert.Single(page.Items);
        Assert.Equal("Electronics", category.Name);

        Assert.Equal(2, page.PageNumber);
        Assert.Equal(2, page.PageCount);

        Assert.False(page.HasNextPage);
        Assert.True(page.HasPreviousPage);
    }
}
