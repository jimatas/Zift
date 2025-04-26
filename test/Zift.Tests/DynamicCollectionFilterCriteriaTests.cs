namespace Zift.Tests;

using Filtering;
using Filtering.Dynamic.Parsing;
using SharedFixture.Models;

public class DynamicCollectionFilterCriteriaTests
{
    [Fact]
    public void Filter_ValidExpression_ReturnsMatchingCategories()
    {
        var filterCriteria = new DynamicFilterCriteria<Category>("Products.Reviews:any == true");

        var result = Catalog.Categories.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void Filter_ImplicitAnyCriteria_ReturnsMatchingCategories()
    {
        var filterCriteria = new DynamicFilterCriteria<Category>("Products.Price >= 20 && Products.Price <= 1000");

        var result = Catalog.Categories.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Equal(3, result.Count);
        Assert.Equal(["Electronics", "Home Appliances", "Clothing"], result.Select(c => c.Name));
    }

    [Fact]
    public void Filter_ExplicitAnyCriteria_ReturnsMatchingCategories()
    {
        var filterCriteria = new DynamicFilterCriteria<Category>("Products:any.Price >= 20 && Products:any.Price <= 1000");

        var result = Catalog.Categories.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Equal(3, result.Count);
        Assert.Equal(["Electronics", "Home Appliances", "Clothing"], result.Select(c => c.Name));
    }

    [Fact]
    public void Filter_ExplicitAllCriteria_ReturnsMatchingCategory()
    {
        var filterCriteria = new DynamicFilterCriteria<Category>("Products:all.Price >= 20 && Products:all.Price <= 1000");

        var result = Catalog.Categories.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Single(result);
        Assert.Equal("Home Appliances", result.Single().Name);
    }

    [Fact]
    public void Filter_NestedPropertyImplicitAny_ReturnsMatchingCategories()
    {
        var filterCriteria = new DynamicFilterCriteria<Category>("Products.Reviews.Rating == 5");

        var result = Catalog.Categories.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Equal(3, result.Count);
        Assert.Equal(["Electronics", "Clothing", "Books"], result.Select(c => c.Name));
    }

    [Fact]
    public void Filter_NestedPropertyExplicitAll_ReturnsMatchingCategory()
    {
        var filterCriteria = new DynamicFilterCriteria<Category>("Products.Reviews:all.Rating == 5");

        var result = Catalog.Categories.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Single(result);
        Assert.Equal("Books", result.Single().Name);
    }

    [Fact]
    public void Filter_AllCriteriaNoMatches_ReturnsEmpty()
    {
        var filterCriteria = new DynamicFilterCriteria<Category>("Products:all.Price < 10");

        var result = Catalog.Categories.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Constructor_InvalidExpression_ThrowsSyntaxErrorException()
    {
        var expression = "Products.Reviews:invalid.Rating == 5";

        var ex = Assert.Throws<SyntaxErrorException>(() => _ = new DynamicFilterCriteria<Category>(expression));

        Assert.StartsWith("Expected a quantifier mode or collection projection, but got: invalid", ex.Message);
    }

    [Theory]
    [InlineData("Products:count == 2", new[] { "Electronics", "Clothing", "Books" })]
    [InlineData("Products:count == 1", new[] { "Home Appliances" })]
    [InlineData("Products:count > 1", new[] { "Electronics", "Clothing", "Books" })]
    [InlineData("Products.Reviews:count == 2", new[] { "Electronics", "Clothing", "Books" })]
    [InlineData("Products.Reviews:count == 1", new[] { "Home Appliances", "Books" })]
    [InlineData("Products.Reviews:count > 1", new[] { "Electronics", "Clothing", "Books" })]
    [InlineData("Products.Reviews:count < 2", new[] { "Home Appliances", "Books" })]
    [InlineData("Products.Reviews:count >= 2", new[] { "Electronics", "Clothing", "Books" })]
    [InlineData("Products.Reviews:count == 0", new string[0])]
    public void Filter_UsingCountProjection_ReturnsMatchingCategories(string expression, IEnumerable<string> expectedCategories)
    {
        var filterCriteria = new DynamicFilterCriteria<Category>(expression);

        var result = Catalog.Categories.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Equal(expectedCategories.Count(), result.Count);
        Assert.True(expectedCategories.All(name => result.Any(c => c.Name == name)));
    }
}
