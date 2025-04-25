namespace Zift.Tests;

using SharedFixture.Models;
using Sorting;

public class SortCriterionTests
{
    [Fact]
    public void Constructor_ValidProperty_SetsExpectedValues()
    {
        var criterion = new SortCriterion<Product>("Name", SortDirection.Descending);

        Assert.Equal("Name", criterion.Property.ToPropertyPath());
        Assert.Equal(SortDirection.Descending, criterion.Direction);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Constructor_NullOrEmptyString_ThrowsArgumentException(string? property)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => new SortCriterion<Product>(property!, SortDirection.Ascending));

        Assert.Equal("property", ex.ParamName);
    }

    [Theory]
    [InlineData("NonExistent")]
    [InlineData("Author.NonExistent")]
    public void Constructor_InvalidProperty_ThrowsArgumentException(string property)
    {
        var ex = Assert.Throws<ArgumentException>(() => new SortCriterion<Review>(property, SortDirection.Ascending));
        Assert.StartsWith("No accessible property", ex.Message);
    }

    [Fact]
    public void ApplyTo_NullQueryable_ThrowsArgumentNullException()
    {
        var criterion = new SortCriterion<Product>("Name", SortDirection.Ascending);
        IQueryable<Product> query = null!;

        var ex = Assert.Throws<ArgumentNullException>(() => criterion.ApplyTo(query));
        Assert.Equal("query", ex.ParamName);
    }

    [Fact]
    public void ApplyTo_NullOrderedQueryable_ThrowsArgumentNullException()
    {
        var criterion = new SortCriterion<Product>("Name", SortDirection.Ascending);
        IOrderedQueryable<Product> sortedQuery = null!;

        var ex = Assert.Throws<ArgumentNullException>(() => criterion.ApplyTo(sortedQuery));
        Assert.Equal("sortedQuery", ex.ParamName);
    }

    [Fact]
    public void ApplyTo_AscendingSortOnProductName_SortsAlphabetically()
    {
        var products = Catalog.Categories.SelectMany(c => c.Products).ToList();
        var criterion = new SortCriterion<Product>("Name", SortDirection.Ascending);

        var result = criterion.ApplyTo(products.AsQueryable()).ToList();

        var expected = products.OrderBy(p => p.Name).Select(p => p.Name).ToList();
        var actual = result.Select(p => p.Name).ToList();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ApplyTo_DescendingSortOnProductPrice_SortsFromHighToLow()
    {
        var products = Catalog.Categories.SelectMany(c => c.Products).ToList();
        var criterion = new SortCriterion<Product>("Price", SortDirection.Descending);

        var result = criterion.ApplyTo(products.AsQueryable()).ToList();

        var expected = products.OrderByDescending(p => p.Price).Select(p => p.Price).ToList();
        var actual = result.Select(p => p.Price).ToList();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ApplyTo_AscendingSortOnReviewAuthorName_SortsByAuthor()
    {
        var reviews = Catalog.Categories
            .SelectMany(c => c.Products)
            .SelectMany(p => p.Reviews)
            .ToList();

        var criterion = new SortCriterion<Review>("Author.Name", SortDirection.Ascending);

        var result = criterion.ApplyTo(reviews.AsQueryable()).ToList();

        var expected = reviews.OrderBy(r => r.Author?.Name).Select(r => r.Author?.Name).ToList();
        var actual = result.Select(r => r.Author?.Name).ToList();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ApplyTo_SortByPropertyWithWrongCase_SucceedsCaseInsensitive()
    {
        var products = Catalog.Categories.SelectMany(c => c.Products).ToList();
        var criterion = new SortCriterion<Product>("nAmE", SortDirection.Ascending);

        var result = criterion.ApplyTo(products.AsQueryable()).ToList();

        var expected = products.OrderBy(p => p.Name).Select(p => p.Name).ToList();
        var actual = result.Select(p => p.Name).ToList();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ApplyTo_ChainedSort_SortsUsingSecondaryCriterion()
    {
        var products = Catalog.Categories.SelectMany(c => c.Products).ToList();

        var primary = new SortCriterion<Product>("Price", SortDirection.Ascending);
        var secondary = new SortCriterion<Product>("Name", SortDirection.Ascending);

        var sorted = primary.ApplyTo(products.AsQueryable());
        var result = secondary.ApplyTo(sorted).ToList();

        var expected = products
            .OrderBy(p => p.Price)
            .ThenBy(p => p.Name)
            .Select(p => (p.Price, p.Name))
            .ToList();

        var actual = result
            .Select(p => (p.Price, p.Name))
            .ToList();

        Assert.Equal(expected, actual);
    }
}
