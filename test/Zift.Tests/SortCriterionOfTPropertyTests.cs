namespace Zift.Tests;

using SharedFixture.Models;
using Sorting;

public class SortCriterionOfTPropertyTests
{
    [Fact]
    public void Constructor_Expression_SetsExpectedValues()
    {
        var criterion = new SortCriterion<Product, string>(p => p.Name!, SortDirection.Ascending);

        Assert.Equal("Name", criterion.Property.ToPropertyPath());
        Assert.Equal(SortDirection.Ascending, criterion.Direction);
    }

    [Fact]
    public void Constructor_NestedExpression_SetsExpectedValues()
    {
        var criterion = new SortCriterion<Review, string>(r => r.Author!.Name!, SortDirection.Ascending);

        Assert.Equal("Author.Name", criterion.Property.ToPropertyPath());
        Assert.Equal(SortDirection.Ascending, criterion.Direction);
    }

    [Fact]
    public void Constructor_UnaryExpression_SetsExpectedPropertyPath()
    {
        Expression<Func<Product, object>> expr = p => (object)p.Name!; // Cast to object to simulate a unary expression
        var criterion = new SortCriterion<Product, object>(expr, SortDirection.Ascending);

        Assert.Equal("Name", criterion.Property.ToPropertyPath());
    }

    [Fact]
    public void Constructor_NullExpression_ThrowsArgumentNullException()
    {
        Expression<Func<Product, string>> expr = null!;
        var ex = Assert.Throws<ArgumentNullException>(() => new SortCriterion<Product, string>(expr, SortDirection.Ascending));

        Assert.Equal("property", ex.ParamName);
    }

    [Fact]
    public void ApplyTo_NullQueryable_ThrowsArgumentNullException()
    {
        var criterion = new SortCriterion<Product, string>(p => p.Name!, SortDirection.Ascending);
        IQueryable<Product> query = null!;

        var ex = Assert.Throws<ArgumentNullException>(() => criterion.ApplyTo(query));
        Assert.Equal("query", ex.ParamName);
    }

    [Fact]
    public void ApplyTo_NullOrderedQueryable_ThrowsArgumentNullException()
    {
        var criterion = new SortCriterion<Product, string>(p => p.Name!, SortDirection.Ascending);
        IOrderedQueryable<Product> sortedQuery = null!;

        var ex = Assert.Throws<ArgumentNullException>(() => criterion.ApplyTo(sortedQuery));
        Assert.Equal("sortedQuery", ex.ParamName);
    }

    [Fact]
    public void ApplyTo_AscendingSortOnProductName_SortsAlphabetically()
    {
        var products = Catalog.Categories.SelectMany(c => c.Products).ToList();
        var criterion = new SortCriterion<Product, string>(p => p.Name!, SortDirection.Ascending);

        var result = criterion.ApplyTo(products.AsQueryable()).ToList();

        var expected = products.OrderBy(p => p.Name).Select(p => p.Name).ToList();
        var actual = result.Select(p => p.Name).ToList();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ApplyTo_DescendingSortOnProductPrice_SortsFromHighToLow()
    {
        var products = Catalog.Categories.SelectMany(c => c.Products).ToList();
        var criterion = new SortCriterion<Product, decimal>(p => p.Price, SortDirection.Descending);

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

        var criterion = new SortCriterion<Review, string>(r => r.Author!.Name!, SortDirection.Ascending);

        var result = criterion.ApplyTo(reviews.AsQueryable()).ToList();

        var expected = reviews.OrderBy(r => r.Author?.Name).Select(r => r.Author?.Name).ToList();
        var actual = result.Select(r => r.Author?.Name).ToList();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ApplyTo_ChainedSort_SortsUsingSecondaryCriterion()
    {
        var products = Catalog.Categories.SelectMany(c => c.Products).ToList();

        var primary = new SortCriterion<Product, decimal>(p => p.Price, SortDirection.Ascending);
        var secondary = new SortCriterion<Product, string>(p => p.Name!, SortDirection.Ascending);

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
