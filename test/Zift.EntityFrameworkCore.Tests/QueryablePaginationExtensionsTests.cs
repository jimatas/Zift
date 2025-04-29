namespace Zift.EntityFrameworkCore.Tests;

using EntityFrameworkCore.Tests.Fixture;
using Pagination;
using SharedFixture.Models;

public class QueryablePaginationExtensionsTests
{
    [Fact]
    public async Task ToPaginatedListAsync_WithNullConfiguration_ThrowsArgumentNullException()
    {
        await using var dbContext = await SqliteDbHelper.CreateDatabaseAsync();

        var query = dbContext.Set<Product>().AsQueryable();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await query.ToPaginatedListAsync((Action<PaginationCriteriaBuilder<Product>>)null!));
    }

    [Fact]
    public async Task ToPaginatedListAsync_WithNullCriteria_ThrowsArgumentNullException()
    {
        await using var dbContext = await SqliteDbHelper.CreateDatabaseAsync();

        var query = dbContext.Set<Product>().AsQueryable();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await query.ToPaginatedListAsync((IPaginationCriteria<Product>)null!));
    }

    [Fact]
    public async Task ToPaginatedListAsync_UsesProvidedPaginationCriteria()
    {
        await using var dbContext = await SqliteDbHelper.CreateDatabaseAsync(context =>
        {
            context.Set<Product>().AddRange(Enumerable.Range(1, 9)
                .Select(i => new Product { Name = $"Product {i}" }));
        });

        var result = await dbContext.Set<Product>()
            .SortBy(sort => sort.Ascending("Name"))
            .ToPaginatedListAsync(c => c.AtPage(2).WithSize(3));

        Assert.Equal(3, result.Count);
        Assert.Equal("Product 4", result[0].Name);
        Assert.Equal("Product 6", result[2].Name);
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
    public async Task ToPaginatedListAsync_SinglePage_ReturnsAllItems()
    {
        await using var dbContext = await SqliteDbHelper.CreateDatabaseAsync(context =>
        {
            context.Set<Product>().AddRange(Enumerable.Range(1, 2)
                .Select(i => new Product { Name = $"Product {i}" }));
        });

        var result = await dbContext.Set<Product>()
            .SortBy(sort => sort.Ascending("Name"))
            .ToPaginatedListAsync(c => c.AtPage(1).WithSize(5));

        Assert.Equal(2, result.Count);
        Assert.Equal("Product 1", result[0].Name);
        Assert.Equal("Product 2", result[1].Name);
    }

    [Fact]
    public async Task ToPaginatedListAsync_EmptyQuery_ReturnsEmptyResult()
    {
        await using var dbContext = await SqliteDbHelper.CreateDatabaseAsync();

        var result = await dbContext.Set<Product>()
            .ToPaginatedListAsync(c => c.AtPage(1).WithSize(5));

        Assert.Empty(result);
    }
}
