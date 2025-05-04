namespace Zift.EntityFrameworkCore.Tests;

using Filtering;
using Fixture;
using Microsoft.EntityFrameworkCore;
using SharedFixture.Models;

public class DynamicFilterCriteriaTests
{
    [Fact]
    public async Task Filter_ByExactProductName_MaterializesCorrectly()
    {
        await using var dbContext = await CreatePopulatedDbContextAsync();

        var filter = new DynamicFilterCriteria<Category>("Products.Name == 'Smartphone'");

        var result = await dbContext.Set<Category>()
            .Filter(filter)
            .ToListAsync();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public async Task Filter_ByProductPrice_GreaterThanThreshold_MaterializesCorrectly()
    {
        await using var dbContext = await CreatePopulatedDbContextAsync();

        var filter = new DynamicFilterCriteria<Category>("Products.Price > 500");

        var result = await dbContext.Set<Category>()
            .Filter(filter)
            .ToListAsync();

        Assert.Contains(result, c => c.Name == "Electronics");
    }

    [Fact]
    public async Task Filter_ByReviewAuthorName_MaterializesCorrectly()
    {
        await using var dbContext = await CreatePopulatedDbContextAsync();

        var filter = new DynamicFilterCriteria<Category>("Products.Reviews.Author.Name == 'John Doe'");

        var result = await dbContext.Set<Category>()
            .Filter(filter)
            .ToListAsync();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public async Task Filter_ByNullableRating_MaterializesCorrectly()
    {
        await using var dbContext = await CreatePopulatedDbContextAsync();

        var filter = new DynamicFilterCriteria<Category>("Products.Reviews.Rating >= 4");

        var result = await dbContext.Set<Category>()
            .Filter(filter)
            .ToListAsync();

        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task Filter_ByProductStartsWithName_MaterializesCorrectly()
    {
        await using var dbContext = await CreatePopulatedDbContextAsync();

        var filter = new DynamicFilterCriteria<Category>("Products.Name ^= 'S'");

        var result = await dbContext.Set<Category>()
            .Filter(filter)
            .ToListAsync();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public async Task Filter_ByAnyProductWithPriceAboveThreshold_MaterializesCorrectly()
    {
        await using var dbContext = await CreatePopulatedDbContextAsync();

        var filter = new DynamicFilterCriteria<Category>("Products:any.Price > 500");

        var result = await dbContext.Set<Category>()
            .Filter(filter)
            .ToListAsync();

        Assert.NotEmpty(result);
        Assert.Contains(result, c => c.Name == "Electronics");
    }

    [Fact]
    public async Task Filter_ByAllProductsWithPriceAboveThreshold_MaterializesCorrectly()
    {
        await using var dbContext = await CreatePopulatedDbContextAsync();

        var filter = new DynamicFilterCriteria<Category>("Products:all.Price > 10");

        var result = await dbContext.Set<Category>()
            .Filter(filter)
            .ToListAsync();

        Assert.NotEmpty(result);
        Assert.Contains(result, c => c.Name == "Home Appliances");
    }

    [Fact]
    public async Task Filter_ByProductCountGreaterThanOrEqualTwo_MaterializesCorrectly()
    {
        await using var dbContext = await CreatePopulatedDbContextAsync();

        var filter = new DynamicFilterCriteria<Category>("Products:count >= 2");

        var result = await dbContext.Set<Category>()
            .Filter(filter)
            .ToListAsync();

        Assert.NotEmpty(result);
        Assert.Contains(result, c => c.Name == "Electronics");
        Assert.Contains(result, c => c.Name == "Clothing");
    }

    [Fact]
    public async Task Filter_ByNonNullReviewAuthorName_MaterializesCorrectly()
    {
        await using var dbContext = await CreatePopulatedDbContextAsync();

        var filter = new DynamicFilterCriteria<Category>("Products.Reviews.Author.Name != null");

        var result = await dbContext.Set<Category>()
            .Filter(filter)
            .ToListAsync();

        Assert.NotEmpty(result);
        Assert.All(result, category =>
        {
            Assert.All(category.Products, product =>
            {
                if (product.Reviews is not null)
                {
                    foreach (var review in product.Reviews)
                    {
                        Assert.NotNull(review.Author?.Name);
                    }
                }
            });
        });
    }

    [Fact]
    public async Task Filter_ByProductNameEqualsIgnoreCase_ReturnsMatchingCategory()
    {
        await using var dbContext = await CreatePopulatedDbContextAsync();

        var filter = new DynamicFilterCriteria<Category>("Products.Name == 'smartphone':i");

        var result = await dbContext.Set<Category>()
            .Filter(filter)
            .ToListAsync();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public async Task Filter_ByProductNameNotEqualsIgnoreCase_ReturnsAllExceptMatch()
    {
        await using var dbContext = await CreatePopulatedDbContextAsync();

        var filter = new DynamicFilterCriteria<Category>("Products:all.Name != 'SMARTPHONE':i");

        var result = await dbContext.Set<Category>()
            .Filter(filter)
            .ToListAsync();

        Assert.DoesNotContain(result, c => c.Products.Any(p => p.Name == "Smartphone"));
    }

    [Fact]
    public async Task Filter_ByProductNameStartsWithIgnoreCase_ReturnsMatchingCategory()
    {
        await using var dbContext = await CreatePopulatedDbContextAsync();

        var filter = new DynamicFilterCriteria<Category>("Products.Name ^= 's':i");

        var result = await dbContext.Set<Category>()
            .Filter(filter)
            .ToListAsync();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public async Task Filter_ByProductNameEndsWithIgnoreCase_ReturnsMatchingCategory()
    {
        await using var dbContext = await CreatePopulatedDbContextAsync();

        var filter = new DynamicFilterCriteria<Category>("Products.Name $= 'TOP':i");

        var result = await dbContext.Set<Category>()
            .Filter(filter)
            .ToListAsync();

        Assert.Single(result);
        Assert.Equal("Electronics", result[0].Name);
    }

    [Fact]
    public async Task Filter_ByProductNameContainsIgnoreCase_ReturnsMatchingCategories()
    {
        await using var dbContext = await CreatePopulatedDbContextAsync();

        var filter = new DynamicFilterCriteria<Category>("Products.Name %= 'EA':i");

        var result = await dbContext.Set<Category>()
            .Filter(filter)
            .ToListAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Name == "Clothing");
        Assert.Contains(result, c => c.Name == "Books");
    }

    [Fact]
    public async Task Filter_ByProductNameEqualsIgnoreCase_ReturnsEmpty()
    {
        await using var dbContext = await CreatePopulatedDbContextAsync();

        var filter = new DynamicFilterCriteria<Category>("Products.Name == 'Unicorn':i");

        var result = await dbContext.Set<Category>()
            .Filter(filter)
            .ToListAsync();

        Assert.Empty(result);
    }

    #region Fixture
    private static async Task<CatalogDbContext> CreatePopulatedDbContextAsync()
    {
        return await SqliteDbHelper.CreateDatabaseAsync(context =>
        {
            context.Set<Category>().AddRange(Catalog.Categories);
        });
    }
    #endregion
}
