namespace Zift.Querying;

using Fixture;
using Microsoft.EntityFrameworkCore;

public sealed class WhereSqliteIntegrationTests
{
    [Fact]
    public async Task Where_CategoryNameEquals_ReturnsMatchingCategory()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var names = await fixture.Context.Categories
            .Where("Name == \"Electronics\"")
            .Select(c => c.Name)
            .ToListAsync();

        Assert.Single(names);
        Assert.Equal("Electronics", names[0]);
    }

    [Fact]
    public async Task Where_ProductPriceGreaterThan_UsesAnyAndReturnsMatchingCategory()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var names = await fixture.Context.Categories
            .Where("Products:any(Price > 1000)")
            .Select(c => c.Name)
            .ToListAsync();

        Assert.Single(names);
        Assert.Equal("Electronics", names[0]);
    }

    [Fact]
    public async Task Where_NestedAnyOnReviewsRating_ReturnsBothCategories()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var names = await fixture.Context.Categories
            .Where("Products:any(Reviews:any(Rating >= 5))")
            .Select(c => c.Name)
            .OrderBy(n => n)
            .ToListAsync();

        Assert.Equal(2, names.Count);
        Assert.Equal("Books", names[0]);
        Assert.Equal("Electronics", names[1]);
    }

    [Fact]
    public async Task Where_NullRatingInsideNestedAny_ReturnsBooks()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var names = await fixture.Context.Categories
            .Where("Products:any(Reviews:any(Rating == null))")
            .Select(c => c.Name)
            .ToListAsync();

        Assert.Single(names);
        Assert.Equal("Books", names[0]);
    }

    [Fact]
    public async Task Where_NullAuthorInsideNestedAny_ReturnsElectronics()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var names = await fixture.Context.Categories
            .Where("Products:any(Reviews:any(Author == null))")
            .Select(c => c.Name)
            .ToListAsync();

        Assert.Single(names);
        Assert.Equal("Electronics", names[0]);
    }

    [Fact]
    public async Task Where_AuthorEmailMatchInsideNestedAny_ReturnsElectronics()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var names = await fixture.Context.Categories
            .Where("Products:any(Reviews:any(Author.Email == \"john.doe@example.com\"))")
            .Select(c => c.Name)
            .ToListAsync();

        Assert.Single(names);
        Assert.Equal("Electronics", names[0]);
    }

    [Fact]
    public async Task Where_DatePostedBeforeTimestamp_ReturnsBooks()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var names = await fixture.Context.Categories
            .Where("Products:any(Reviews:any(DatePosted < \"2024-01-01T00:00:00Z\"))")
            .Select(c => c.Name)
            .ToListAsync();

        Assert.Single(names);
        Assert.Equal("Books", names[0]);
    }

    [Fact]
    public async Task Where_CompoundPredicateAcrossNestedProperties_ReturnsElectronics()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var names = await fixture.Context.Categories
            .Where(
                "Products:any(Price > 1000) && " +
                "Products:any(Reviews:any(Rating <= 3))")
            .Select(c => c.Name)
            .ToListAsync();

        Assert.Single(names);
        Assert.Equal("Electronics", names[0]);
    }

    [Fact]
    public async Task Where_NoCategoryMatches_ReturnsEmpty()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var result = await fixture.Context.Categories
            .Where("Name == \"NonExisting\"")
            .ToListAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task Where_NestedPredicateWithNoMatch_ReturnsEmpty()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var result = await fixture.Context.Categories
            .Where("Products:any(Reviews:any(Rating > 10))")
            .ToListAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task Where_MixedNullAndNonNullPredicates_ReturnsBothCategories()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var names = await fixture.Context.Categories
            .Where(
                "Products:any(Reviews:any(Rating != null && Rating >= 4))")
            .Select(c => c.Name)
            .OrderBy(n => n)
            .ToListAsync();

        Assert.Equal(2, names.Count);
        Assert.Equal("Books", names[0]);
        Assert.Equal("Electronics", names[1]);
    }

    [Fact]
    public async Task Where_DatePostedEqualsExactValue_ReturnsBooks()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var names = await fixture.Context.Categories
            .Where(
                "Products:any(Reviews:any(DatePosted == \"2023-11-20T00:00:00Z\"))")
            .Select(c => c.Name)
            .ToListAsync();

        Assert.Single(names);
        Assert.Equal("Books", names[0]);
    }

    [Fact]
    public async Task Where_StringComparisonOnNestedProperty_ReturnsElectronics()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var names = await fixture.Context.Categories
            .Where(
                "Products:any(Reviews:any(Content == \"Great phone!\"))")
            .Select(c => c.Name)
            .ToListAsync();

        Assert.Single(names);
        Assert.Equal("Electronics", names[0]);
    }

    [Fact]
    public async Task Where_NegatedNestedPredicate_ReturnsBooks()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var names = await fixture.Context.Categories
            .Where(
                "!Products:any(Reviews:any(Rating < 3))")
            .Select(c => c.Name)
            .ToListAsync();

        Assert.Single(names);
        Assert.Equal("Books", names[0]);
    }

    [Fact]
    public async Task Where_OperatorPrecedence_ReturnsExpectedCategories()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var names = await fixture.Context.Categories
            .Where(
                "Name == \"Books\" || Name == \"Electronics\" && Products:any(Price > 1000)")
            .Select(c => c.Name)
            .OrderBy(n => n)
            .ToListAsync();

        Assert.Equal(2, names.Count);
    }

    [Fact]
    public async Task Where_RedundantPredicates_AreIdempotent()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var names = await fixture.Context.Categories
            .Where("Name == \"Electronics\" && Name == \"Electronics\"")
            .Select(c => c.Name)
            .ToListAsync();

        Assert.Single(names);
        Assert.Equal("Electronics", names[0]);
    }

    [Fact]
    public async Task Where_AnyWithoutPredicate_OnProducts_ReturnsAllCategories()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var names = await fixture.Context.Categories
            .Where("Products:any()")
            .Select(c => c.Name)
            .OrderBy(n => n)
            .ToListAsync();

        Assert.Equal(2, names.Count);
        Assert.Equal("Books", names[0]);
        Assert.Equal("Electronics", names[1]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Where_NullOrWhiteSpaceExpression_ReturnsAllCategories(string? whereExpression)
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var categories = await fixture.Context.Categories
            .Where(whereExpression!)
            .ToListAsync();

        Assert.Equal(2, categories.Count);
    }

    [Fact]
    public async Task Where_AllQuantifier_OnProducts_ReturnsAllCategories()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var names = await fixture.Context.Categories
            .Where("Products:all(Price > 10)")
            .Select(c => c.Name)
            .OrderBy(n => n)
            .ToListAsync();

        Assert.Equal(2, names.Count);
        Assert.Equal("Books", names[0]);
        Assert.Equal("Electronics", names[1]);
    }

    [Fact]
    public async Task Where_CountProjection_OnProducts_ReturnsAllCategories()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var names = await fixture.Context.Categories
            .Where("Products:count >= 2")
            .Select(c => c.Name)
            .OrderBy(n => n)
            .ToListAsync();

        Assert.Equal(2, names.Count);
        Assert.Equal("Books", names[0]);
        Assert.Equal("Electronics", names[1]);
    }

    [Fact]
    public async Task Where_InOperator_OnCategoryName_ReturnsMatchingCategory()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var names = await fixture.Context.Categories
            .Where("Name in [\"Books\", \"NonExisting\"]")
            .Select(c => c.Name)
            .ToListAsync();

        Assert.Single(names);
        Assert.Equal("Books", names[0]);
    }

    [Fact]
    public async Task Where_OnProducts_WithNestedAny_ReturnsMatchingProducts()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var productNames = await fixture.Context.Products
            .Where("Reviews:any(Rating >= 5)")
            .Select(p => p.Name)
            .OrderBy(n => n)
            .ToListAsync();

        Assert.Equal(3, productNames.Count);
        Assert.Equal("Laptop", productNames[0]);
        Assert.Equal("Smartphone", productNames[1]);
        Assert.Equal("The Great Gatsby", productNames[2]);
    }
}
