namespace Zift.Tests;

using Filtering;
using SharedFixture.Models;

public class DynamicFilterCriteriaTests
{
    [Fact]
    public void Filter_ByExactProductName_ReturnsMatchingCategory()
    {
        var filterCriteria = new DynamicFilterCriteria<Category>("Products.Name == 'Smartphone'");

        var result = Catalog.Categories.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public void Filter_ByProductNamePrefix_ReturnsMatchingCategory()
    {
        var filterCriteria = new DynamicFilterCriteria<Category>("Products.Name ^= 'S'");

        var result = Catalog.Categories.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public void Filter_ByReviewAuthorSubstring_ReturnsMatchingCategories()
    {
        var filterCriteria = new DynamicFilterCriteria<Category>("Products.Reviews.Author.Name %= 'ee'");

        var result = Catalog.Categories.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Name == "Clothing");
        Assert.Contains(result, c => c.Name == "Books");
    }

    [Fact]
    public void Filter_ByProductPriceRange_ReturnsMatchingCategories()
    {
        var filterCriteria = new DynamicFilterCriteria<Category>("Products.Price >= 500 && Products.Price <= 1300");

        var result = Catalog.Categories.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains("Electronics", result.Select(c => c.Name));
        Assert.Contains("Home Appliances", result.Select(c => c.Name));
    }

    [Fact]
    public void Filter_ByProductPriceRangeWithMixedTypes_ReturnsMatchingCategories()
    {
        var filterCriteria = new DynamicFilterCriteria<Category>("Products.Price >= 500.0 && Products.Price <= '1300'");

        var result = Catalog.Categories.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains("Electronics", result.Select(c => c.Name));
        Assert.Contains("Home Appliances", result.Select(c => c.Name));
    }

    [Fact]
    public void Filter_ByReviewAuthor_ReturnsMatchingCategory()
    {
        var filterCriteria = new DynamicFilterCriteria<Category>("Products.Reviews.Author.Name == 'John Doe'");

        var result = Catalog.Categories.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public void Filter_ByExactProductId_ReturnsMatchingCategory()
    {
        var categories = Catalog.Categories.ToList();
        var productId = categories
            .SelectMany(c => c.Products)
            .First(p => p.Name == "Smartphone").Id;

        var filterCriteria = new DynamicFilterCriteria<Category>($"Products.Id == '{productId}'");

        var result = categories.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public void Filter_ByProductIdExclusion_ReturnsAllCategories()
    {
        var categories = Catalog.Categories.ToList();
        var unrelatedId = Guid.NewGuid();
        var filterCriteria = new DynamicFilterCriteria<Category>($"Products.Id != '{unrelatedId}'");

        var result = categories.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Equal(categories.Count, result.Count);
    }

    [Fact]
    public void Filter_ByPostReviewDate_ReturnsMatchingReview()
    {
        var reviews = new[]
        {
            new Review { Author = new() { Name = "Alice" }, DatePosted = new DateTime(2024, 2, 2) }
        };

        var filterCriteria = new DynamicFilterCriteria<Review>("DatePosted > '2024-01-01'");

        var result = reviews.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Single(result);
        Assert.Equal(new DateTime(2024, 2, 2), result[0].DatePosted);
    }

    [Fact]
    public void Filter_WithinReviewDateRange_ReturnsMatchingReview()
    {
        var reviews = new[]
        {
            new Review { Author = new() { Name = "Bob" }, DatePosted = new DateTime(2024, 2, 2) }
        };

        var filterCriteria = new DynamicFilterCriteria<Review>("DatePosted >= '2024-01-01' && DatePosted <= '2024-12-31'");

        var result = reviews.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Single(result);
        Assert.Equal(new DateTime(2024, 2, 2), result[0].DatePosted);
    }

    [Fact]
    public void Filter_ByNonNullReviewDates_ReturnsMatchingReview()
    {
        var reviews = new[]
        {
            new Review { Author = new() { Name = "Charlie" }, DatePosted = new DateTime(2024, 2, 2) }
        };

        var filterCriteria = new DynamicFilterCriteria<Review>("DatePosted != null");

        var result = reviews.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Single(result);
        Assert.Equal(new DateTime(2024, 2, 2), result[0].DatePosted);
    }

    [Fact]
    public void Filter_ByPreReviewDate_ReturnsNoReviews()
    {
        var reviews = new[]
        {
            new Review { Author = new() { Name = "Sophia" }, DatePosted = new DateTime(2024, 2, 2) }
        };

        var filterCriteria = new DynamicFilterCriteria<Review>("DatePosted < '2024-01-01'");

        var result = reviews.AsQueryable().Filter(filterCriteria).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Filter_ByProductName_EqualIgnoreCase_ReturnsMatchingCategory()
    {
        var filter = new DynamicFilterCriteria<Category>("Products.Name == 'SMARTPHONE':i");

        var result = Catalog.Categories.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public void Filter_ByProductName_NotEqualIgnoreCase_ReturnsAllExceptMatch()
    {
        var filter = new DynamicFilterCriteria<Category>("Products:all.Name != 'SMARTPHONE':i");

        var result = Catalog.Categories.AsQueryable().Filter(filter).ToList();

        Assert.DoesNotContain(result, c => c.Products.Any(p => p.Name == "Smartphone"));
    }

    [Fact]
    public void Filter_ByProductNamePrefix_IgnoreCase_ReturnsMatchingCategory()
    {
        var filter = new DynamicFilterCriteria<Category>("Products.Name ^= 's':i");

        var result = Catalog.Categories.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public void Filter_ByProductNameSuffix_IgnoreCase_ReturnsMatchingCategory()
    {
        var filter = new DynamicFilterCriteria<Category>("Products.Name $= 'PHONE':i");

        var result = Catalog.Categories.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public void Filter_ByProductNameContains_IgnoreCase_ReturnsMatchingCategory()
    {
        var filter = new DynamicFilterCriteria<Category>("Products.Name %= 'marT':i");

        var result = Catalog.Categories.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }
}
