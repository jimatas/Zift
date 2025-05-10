namespace Zift.Tests;

using Pagination;
using SharedFixture.Models;

public class QueryablePaginationExtensionsTests
{
    [Fact]
    public void ToPaginatedList_WithNullCriteria_ThrowsArgumentNullException()
    {
        var query = new[] { new Product() }.AsQueryable();

        Assert.Throws<ArgumentNullException>("pagination",
            () => query.ToPaginatedList(null!));
    }

    [Fact]
    public void ToPaginatedList_WithExplicitPaginationCriteria_ReturnsExpectedPage()
    {
        var query = Enumerable.Range(1, 5)
            .Select(i => new Product { Name = $"Product {i}" })
            .AsQueryable();

        var criteria = new PaginationCriteria<Product>(2, 2);
        var result = query.ToPaginatedList(criteria);

        Assert.Equal(2, result.Count);
        Assert.Equal("Product 3", result[0].Name);
        Assert.Equal("Product 4", result[1].Name);
    }

    [Fact]
    public void ToPaginatedList_WithoutPageNumberAndSize_UsesDefaults()
    {
        var query = Enumerable.Range(1, 100)
            .Select(i => new Product { Name = $"Product {i:D3}" })
            .AsQueryable();

        var result = query.ToPaginatedList();

        Assert.Equal(PaginationCriteria<Product>.DefaultPageSize, result.Count);
        Assert.Equal("Product 001", result[0].Name);
    }

    [Fact]
    public void ToPaginatedList_WithExplicitPageNumberAndSize_ReturnsCorrectSubset()
    {
        var query = Enumerable.Range(1, 10)
            .Select(i => new Product { Name = $"Product {i:D2}" })
            .AsQueryable();

        var result = query.ToPaginatedList(pageNumber: 2, pageSize: 3);

        Assert.Equal(3, result.Count);
        Assert.Equal("Product 04", result[0].Name);
        Assert.Equal("Product 06", result[2].Name);
    }

    [Fact]
    public void ToPaginatedList_EmptyQuery_ReturnsEmptyResult()
    {
        var query = Enumerable.Empty<Product>().AsQueryable();

        var result = query.ToPaginatedList(1, 5);

        Assert.Empty(result);
    }
}
