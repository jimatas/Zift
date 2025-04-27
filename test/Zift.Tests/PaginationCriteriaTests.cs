namespace Zift.Tests;

using Pagination;
using SharedFixture.Models;

public class PaginationCriteriaTests
{
    [Fact]
    public void Constructor_WithoutParameters_SetsDefaultValues()
    {
        var criteria = new PaginationCriteria<Product>();

        Assert.Equal(1, criteria.PageNumber);
        Assert.Equal(PaginationCriteria<Product>.DefaultPageSize, criteria.PageSize);
    }

    [Fact]
    public void Constructor_ValidValues_SetsExpectedValues()
    {
        var criteria = new PaginationCriteria<Product>(pageNumber: 3, pageSize: 10);

        Assert.Equal(3, criteria.PageNumber);
        Assert.Equal(10, criteria.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_InvalidPageNumber_ThrowsArgumentOutOfRangeException(int pageNumber)
    {
        Assert.Throws<ArgumentOutOfRangeException>("PageNumber", () => new PaginationCriteria<Product>(pageNumber));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidPageSize_ThrowsArgumentOutOfRangeException(int pageSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>("PageSize", () => new PaginationCriteria<Product>(1, pageSize));
    }

    [Fact]
    public void SetPageNumber_InvalidValue_ThrowsArgumentOutOfRangeException()
    {
        var criteria = new PaginationCriteria<Product>();

        Assert.Throws<ArgumentOutOfRangeException>("PageNumber", () => criteria.PageNumber = 0);
    }

    [Fact]
    public void SetPageSize_InvalidValue_ThrowsArgumentOutOfRangeException()
    {
        var criteria = new PaginationCriteria<Product>();

        Assert.Throws<ArgumentOutOfRangeException>("PageSize", () => criteria.PageSize = 0);
    }

    [Fact]
    public void ApplyTo_NullQuery_ThrowsArgumentNullException()
    {
        var criteria = new PaginationCriteria<Product>();

        Assert.Throws<ArgumentNullException>("query", () => criteria.ApplyTo(null!));
    }

    [Fact]
    public void ApplyTo_WithPagination_ReturnsCorrectSubset()
    {
        var products = Enumerable.Range(1, 20)
            .Select(i => new Product { Name = $"Product {i}" })
            .AsQueryable();

        var criteria = new PaginationCriteria<Product>(pageNumber: 2, pageSize: 5);

        var result = criteria.ApplyTo(products).ToList();

        Assert.Equal(5, result.Count);
        Assert.Equal("Product 6", result.First().Name);
        Assert.Equal("Product 10", result.Last().Name);
    }

    [Fact]
    public void ApplyTo_PageNumberBeyondTotalPages_ReturnsEmpty()
    {
        var products = Enumerable.Range(1, 5)
            .Select(i => new Product { Name = $"Product {i}" })
            .AsQueryable();

        var criteria = new PaginationCriteria<Product>(pageNumber: 3, pageSize: 5);

        var result = criteria.ApplyTo(products).ToList();

        Assert.Empty(result);
    }
}
