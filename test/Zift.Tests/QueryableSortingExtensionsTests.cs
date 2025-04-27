namespace Zift.Tests;

using SharedFixture.Models;
using Sorting;

public class QueryableSortingExtensionsTests
{
    [Fact]
    public void SortBy_NullSortCriteria_ThrowsArgumentNullException()
    {
        var query = new[] { new Product() }.AsQueryable();

        Assert.Throws<ArgumentNullException>("sortCriteria", () => query.SortBy((ISortCriteria<Product>)null!));
    }

    [Fact]
    public void SortBy_ValidCriteria_ReturnsSortedQuery()
    {
        var products = new[]
        {
            new Product { Name = "Banana" },
            new Product { Name = "Apple" },
            new Product { Name = "Cherry" }
        }.AsQueryable();

        var sortCriteria = new SortCriteria<Product>
        {
            new SortCriterion<Product, string?>(p => p.Name, SortDirection.Ascending)
        };

        var result = products.SortBy(sortCriteria).ToList();
        
        Assert.Equal("Apple", result[0].Name);
        Assert.Equal("Banana", result[1].Name);
        Assert.Equal("Cherry", result[2].Name);
    }

    [Fact]
    public void SortBy_NullConfiguration_ThrowsArgumentNullException()
    {
        var query = new[] { new Product() }.AsQueryable();

        Assert.Throws<ArgumentNullException>("configureSorting", () => query.SortBy((Action<SortCriteriaBuilder<Product>>)null!));
    }

    [Fact]
    public void SortBy_ValidConfiguration_ReturnsSortedQuery()
    {
        var products = new[]
        {
            new Product { Name = "Banana" },
            new Product { Name = "Apple" },
            new Product { Name = "Cherry" }
        }.AsQueryable();

        var result = products.SortBy(c => c.Ascending(p => p.Name)).ToList();

        Assert.Equal("Apple", result[0].Name);
        Assert.Equal("Banana", result[1].Name);
        Assert.Equal("Cherry", result[2].Name);
    }
}
