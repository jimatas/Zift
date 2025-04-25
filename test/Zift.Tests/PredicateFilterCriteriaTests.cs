namespace Zift.Tests;

using Filtering;
using SharedFixture.Models;

public class PredicateFilterCriteriaTests
{
    [Fact]
    public void Constructor_NullPredicate_ThrowsArgumentNullException()
    {
        Expression<Func<object, bool>>? predicate = null;

        Assert.Throws<ArgumentNullException>("predicate", () => new PredicateFilterCriteria<object>(predicate!));
    }

    [Fact]
    public void ApplyTo_ValidPredicate_ReturnsOnlyMatchingElements()
    {
        var products = new[]
        {
            new Product { Name = "Product 1" },
            new Product { Name = "Product 2" },
            new Product { Name = "Product 3" }
        }.AsQueryable();

        var criteria = new PredicateFilterCriteria<Product>(p => p.Name == "Product 2");

        var result = criteria.ApplyTo(products);

        Assert.Single(result);
        Assert.All(result, item => Assert.Equal("Product 2", item.Name));
    }

    [Fact]
    public void ApplyTo_DoesNotModifyOriginalQuery()
    {
        var original = new[]
        {
            new Product { Name = "A" },
            new Product { Name = "B" }
        }.AsQueryable();

        var criteria = new PredicateFilterCriteria<Product>(p => p.Name == "B");

        var result = criteria.ApplyTo(original);

        Assert.NotSame(original, result);
    }
}
