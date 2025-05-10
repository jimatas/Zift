namespace Zift.EntityFrameworkCore.Tests;

using EntityFrameworkCore.Tests.Fixture;
using Pagination;
using SharedFixture.Models;

public class QueryablePaginationExtensionsTests
{
    [Fact]
    public async Task ToPaginatedListAsync_WithNullCriteria_ThrowsArgumentNullException()
    {
        await using var dbContext = await SqliteDbHelper.CreateDatabaseAsync();

        var query = dbContext.Set<Product>().AsQueryable();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await query.ToPaginatedListAsync(null!));
    }

    [Fact]
    public async Task ToPaginatedListAsync_WithExplicitPaginationCriteria_ReturnsExpectedPage()
    {
        await using var dbContext = await SqliteDbHelper.CreateDatabaseAsync(context =>
        {
            context.Set<Product>().AddRange(Enumerable.Range(1, 5)
                .Select(i => new Product { Name = $"Product {i}" }));
        });

        var criteria = new PaginationCriteria<Product>(2, 2);

        var result = await dbContext.Set<Product>()
            .SortBy(sort => sort.Ascending("Name"))
            .ToPaginatedListAsync(criteria);

        Assert.Equal(2, result.Count);
        Assert.Equal("Product 3", result[0].Name);
        Assert.Equal("Product 4", result[1].Name);
    }

    [Fact]
    public async Task ToPaginatedListAsync_WithoutPageNumberAndSize_UsesDefaults()
    {
        await using var dbContext = await SqliteDbHelper.CreateDatabaseAsync(context =>
        {
            context.Set<Product>().AddRange(Enumerable.Range(1, 100)
                .Select(i => new Product { Name = $"Product {i:D3}" }));
        });

        var result = await dbContext.Set<Product>()
            .SortBy(sort => sort.Ascending("Name"))
            .ToPaginatedListAsync();

        Assert.Equal(PaginationCriteria<Product>.DefaultPageSize, result.Count);
        Assert.Equal("Product 001", result[0].Name);
    }

    [Fact]
    public async Task ToPaginatedListAsync_WithExplicitPageNumberAndSize_ReturnsCorrectSubset()
    {
        await using var dbContext = await SqliteDbHelper.CreateDatabaseAsync(context =>
        {
            context.Set<Product>().AddRange(Enumerable.Range(1, 10)
                .Select(i => new Product { Name = $"Product {i:D2}" }));
        });

        var result = await dbContext.Set<Product>()
            .SortBy(sort => sort.Ascending("Name"))
            .ToPaginatedListAsync(pageNumber: 2, pageSize: 3);

        Assert.Equal(3, result.Count);
        Assert.Equal("Product 04", result[0].Name);
        Assert.Equal("Product 06", result[2].Name);
    }

    [Fact]
    public async Task ToPaginatedListAsync_SinglePage_ReturnsAllItems()
    {
        await using var dbContext = await SqliteDbHelper.CreateDatabaseAsync(context =>
        {
            context.Set<Product>().AddRange(Enumerable.Range(1, 2)
                .Select(i => new Product { Name = $"Product {i}" }));
        });

        var result = await dbContext.Set<Product>()
            .SortBy(sort => sort.Ascending("Name"))
            .ToPaginatedListAsync(pageNumber: 1, pageSize: 5);

        Assert.Equal(2, result.Count);
        Assert.Equal("Product 1", result[0].Name);
        Assert.Equal("Product 2", result[1].Name);
    }

    [Fact]
    public async Task ToPaginatedListAsync_EmptyQuery_ReturnsEmptyResult()
    {
        await using var dbContext = await SqliteDbHelper.CreateDatabaseAsync();

        var result = await dbContext.Set<Product>()
            .ToPaginatedListAsync(pageNumber: 1, pageSize: 5);

        Assert.Empty(result);
    }
}
