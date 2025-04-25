namespace Zift.Tests;

using Pagination;
using SharedFixture.Models;
using Sorting;

public class PaginationCriteriaTests
{
    [Fact]
    public void Constructor_WithoutParameters_SetsDefaultValues()
    {
        var criteria = new PaginationCriteria<Product>();

        Assert.Equal(1, criteria.PageNumber);
        Assert.Equal(PaginationCriteria<Product>.DefaultPageSize, criteria.PageSize);
        Assert.Empty(criteria.SortCriteria);
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
    public void SortCriteria_AsReadOnlyList_ReturnsCopy()
    {
        var criteria = new PaginationCriteria<Product>();
        criteria.SortCriteria.Add(new SortCriterion<Product>("Name", SortDirection.Ascending));

        var paginationCriteria = (IPaginationCriteria<Product>)criteria;
        var readOnly = paginationCriteria.SortCriteria;

        Assert.Single(readOnly);
        Assert.NotSame(readOnly, criteria.SortCriteria);
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
    public void ApplyTo_WithSortCriteria_AppliesSortingBeforePaging()
    {
        var products = new[]
        {
            new Product { Name = "Zebra" },
            new Product { Name = "Apple" },
            new Product { Name = "Banana" }
        }.AsQueryable();

        var criteria = new PaginationCriteria<Product>(pageNumber: 1, pageSize: 2);
        criteria.SortCriteria.Add(new SortCriterion<Product>("Name", SortDirection.Ascending));

        var result = criteria.ApplyTo(products).ToList();

        Assert.Equal("Apple", result.First().Name);
        Assert.Equal("Banana", result.Last().Name);
    }

    [Fact]
    public void ApplyTo_WithMultipleSortCriteria_AppliesAllInOrder()
    {
        var products = new[]
        {
            new Product { Name = "Product 2", Price = 10 },
            new Product { Name = "Product 1", Price = 10 },
            new Product { Name = "Product 1", Price = 20 }
        }.AsQueryable();

        var criteria = new PaginationCriteria<Product>(pageNumber: 1, pageSize: 10);
        criteria.SortCriteria.Add(new SortCriterion<Product>("Name", SortDirection.Ascending));
        criteria.SortCriteria.Add(new SortCriterion<Product>("Price", SortDirection.Ascending));

        var result = criteria.ApplyTo(products).ToList();

        var expected = products
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Price)
            .ToList();

        Assert.Equal(expected.Select(p => (p.Name, p.Price)), result.Select(p => (p.Name, p.Price)));
    }

    [Fact]
    public void ApplyTo_WithoutSortCriteria_UsesOriginalOrder()
    {
        var products = new[]
        {
            new Product { Name = "Product 1" },
            new Product { Name = "Product 2" },
            new Product { Name = "Product 3" }
        }.AsQueryable();

        var criteria = new PaginationCriteria<Product>(pageNumber: 1, pageSize: 2);

        var result = criteria.ApplyTo(products).ToList();

        Assert.Equal("Product 1", result.First().Name);
        Assert.Equal("Product 2", result.Last().Name);
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
