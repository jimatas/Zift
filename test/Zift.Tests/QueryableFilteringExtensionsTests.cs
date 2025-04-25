namespace Zift.Tests;

using Filtering;
using SharedFixture.Models;

public class QueryableFilteringExtensionsTests
{
    [Fact]
    public void Filter_NullFilter_ThrowsArgumentNullException()
    {
        var query = new[] { new Product() }.AsQueryable();

        Assert.Throws<ArgumentNullException>("filter", () => query.Filter(null!));
    }

    [Fact]
    public void Filter_ValidCriteria_ReturnsFilteredQuery()
    {
        var products = new[]
        {
            new Product { Name = "Product 1" },
            new Product { Name = "Product 2" }
        }.AsQueryable();

        var filter = new PredicateFilterCriteria<Product>(p => p.Name == "Product 1");

        var result = products.Filter(filter).ToList();

        var product = Assert.Single(result);
        Assert.Equal("Product 1", product.Name);
    }

    [Fact]
    public void Filter_EmptyQuery_ReturnsEmptyResult()
    {
        var products = Enumerable.Empty<Product>().AsQueryable();
        
        var filter = new PredicateFilterCriteria<Product>(p => p.Name == "Product 1");
        
        var result = products.Filter(filter).ToList();
        
        Assert.Empty(result);
    }

    [Fact]
    public void Filter_CanBeChainedWithOtherLinqCalls()
    {
        var products = new[]
        {
            new Product { Name = "Cherry" },
            new Product { Name = "Apple" },
            new Product { Name = "Banana" },
            new Product { Name = "Avocado" }
        }.AsQueryable();

        var filter = new PredicateFilterCriteria<Product>(p => p.Name!.StartsWith('A'));

        var result = products
            .Filter(filter)
            .OrderBy(p => p.Name)
            .Select(p => p.Name)
            .ToList();

        Assert.Equal(new[] { "Apple", "Avocado" }, result);
    }
}
