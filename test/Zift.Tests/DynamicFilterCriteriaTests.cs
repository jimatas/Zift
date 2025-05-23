﻿namespace Zift.Tests;

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
    public void Filter_ByProductNameEqualIgnoreCase_ReturnsMatchingCategory()
    {
        var filter = new DynamicFilterCriteria<Category>("Products.Name ==:i 'SMARTPHONE'");

        var result = Catalog.Categories.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public void Filter_ByProductNameNotEqualIgnoreCase_ReturnsAllExceptMatch()
    {
        var filter = new DynamicFilterCriteria<Category>("Products:all.Name !=:i 'SMARTPHONE'");

        var result = Catalog.Categories.AsQueryable().Filter(filter).ToList();

        Assert.DoesNotContain(result, c => c.Products.Any(p => p.Name == "Smartphone"));
    }

    [Fact]
    public void Filter_ByProductNamePrefixIgnoreCase_ReturnsMatchingCategory()
    {
        var filter = new DynamicFilterCriteria<Category>("Products.Name ^=:i 's'");

        var result = Catalog.Categories.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public void Filter_ByProductNameSuffixIgnoreCase_ReturnsMatchingCategory()
    {
        var filter = new DynamicFilterCriteria<Category>("Products.Name $=:i 'PHONE'");

        var result = Catalog.Categories.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public void Filter_ByProductNameContainsIgnoreCase_ReturnsMatchingCategory()
    {
        var filter = new DynamicFilterCriteria<Category>("Products.Name %=:i 'marT'");

        var result = Catalog.Categories.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public void Filter_ByProductNameInList_ReturnsMatchingCategory()
    {
        var filter = new DynamicFilterCriteria<Category>("Products.Name in ['Smartphone', 'Tablet']");

        var result = Catalog.Categories.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public void Filter_ByProductNameInListWhenNoMatch_ReturnsEmpty()
    {
        var filter = new DynamicFilterCriteria<Category>("Products.Name in ['NonExistent']");

        var result = Catalog.Categories.AsQueryable().Filter(filter).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Filter_ByProductNameInListIgnoreCase_ReturnsMatchingCategory()
    {
        var filter = new DynamicFilterCriteria<Category>("Products.Name in:i ['SMARTPHONE', 'TABLET']");

        var result = Catalog.Categories.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public void Filter_ByProductIdInList_ReturnsMatchingCategory()
    {
        var categories = Catalog.Categories.ToList();
        var ids = categories
            .SelectMany(c => c.Products)
            .Where(p => p.Name == "Smartphone" || p.Name == "Tablet")
            .Select(p => p.Id)
            .ToList();

        var idList = $"[{string.Join(", ", ids.Select(id => $"'{id}'"))}]";
        var filter = new DynamicFilterCriteria<Category>($"Products.Id in {idList}");

        var result = categories.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public void Filter_ByProductIdInList_ReturnsMatchingProducts()
    {
        var categories = Catalog.Categories.ToList();
        var ids = categories.SelectMany(c => c.Products)
            .Where(p => p.Name == "Smartphone" || p.Name == "Refrigerator")
            .Select(p => p.Id)
            .ToList();

        var idList = $"[{string.Join(", ", ids.Select(id => $"'{id}'"))}]";
        var filter = new DynamicFilterCriteria<Product>($"Id in {idList}");

        var result = categories.SelectMany(c => c.Products).AsQueryable().Filter(filter).ToList();

        Assert.Equal(2, result.Count);
    }
}
