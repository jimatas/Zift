namespace Zift.Tests;

using Pagination;
using SharedFixture.Models;

public class QueryablePaginationExtensionsTests
{
    [Fact]
    public void ToPaginatedList_WithNullConfiguration_ThrowsArgumentNullException()
    {
        var query = new[] { new Product() }.AsQueryable();

        Assert.Throws<ArgumentNullException>("configurePagination",
            () => query.ToPaginatedList((Action<PaginationCriteriaBuilder<Product>>)null!));
    }

    [Fact]
    public void ToPaginatedList_WithNullCriteria_ThrowsArgumentNullException()
    {
        var query = new[] { new Product() }.AsQueryable();

        Assert.Throws<ArgumentNullException>("paginationCriteria",
            () => query.ToPaginatedList((IPaginationCriteria<Product>)null!));
    }

    [Fact]
    public void ToPaginatedList_UsesProvidedPaginationCriteria()
    {
        var query = Enumerable.Range(1, 10)
            .Select(i => new Product { Name = $"Product {i}" })
            .AsQueryable();

        var result = query.ToPaginatedList(c => c.StartAt(2).WithPageSize(3));

        Assert.Equal(3, result.Count);
        Assert.Equal("Product 4", result[0].Name);
        Assert.Equal("Product 6", result[2].Name);
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
    public void ToPaginatedList_SinglePage_ReturnsAllItems()
    {
        var query = Enumerable.Range(1, 2)
            .Select(i => new Product { Name = $"Product {i}" })
            .AsQueryable();

        var result = query.ToPaginatedList(c => c.StartAt(1).WithPageSize(5));

        Assert.Equal(2, result.Count);
        Assert.Equal("Product 1", result[0].Name);
        Assert.Equal("Product 2", result[1].Name);
    }

    [Fact]
    public void ToPaginatedList_EmptyQuery_ReturnsEmptyResult()
    {
        var query = Enumerable.Empty<Product>().AsQueryable();
        
        var result = query.ToPaginatedList(c => c.StartAt(1).WithPageSize(5));

        Assert.Empty(result);
    }
}
