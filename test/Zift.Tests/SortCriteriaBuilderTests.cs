namespace Zift.Tests;

using SharedFixture.Models;
using Sorting;
using Sorting.Dynamic;

public class SortCriteriaBuilderTests
{
    [Fact]
    public void Ascending_StringProperty_AddsSortCriterion()
    {
        var criteria = new SortCriteria<Product>();
        var builder = new SortCriteriaBuilder<Product>(criteria);

        builder.Ascending("Name");

        var criterion = Assert.Single(criteria);
        Assert.Equal("Name", ((ISortCriterion)criterion).Property);
        Assert.Equal(SortDirection.Ascending, criterion.Direction);
    }

    [Fact]
    public void Ascending_Expression_AddsSortCriterion()
    {
        var criteria = new SortCriteria<Product>();
        var builder = new SortCriteriaBuilder<Product>(criteria);

        builder.Ascending(p => p.Price);

        var criterion = Assert.Single(criteria);
        Assert.Equal("Price", ((ISortCriterion)criterion).Property);
        Assert.Equal(SortDirection.Ascending, criterion.Direction);
    }

    [Fact]
    public void Descending_StringProperty_AddsSortCriterion()
    {
        var criteria = new SortCriteria<Product>();
        var builder = new SortCriteriaBuilder<Product>(criteria);

        builder.Descending("Name");
        
        var criterion = Assert.Single(criteria);
        Assert.Equal("Name", ((ISortCriterion)criterion).Property);
        Assert.Equal(SortDirection.Descending, criterion.Direction);
    }

    [Fact]
    public void Descending_Expression_AddsSortCriterion()
    {
        var criteria = new SortCriteria<Product>();
        var builder = new SortCriteriaBuilder<Product>(criteria);

        builder.Descending(p => p.Price);

        var criterion = Assert.Single(criteria);
        Assert.Equal("Price", ((ISortCriterion)criterion).Property);
        Assert.Equal(SortDirection.Descending, criterion.Direction);
    }

    [Fact]
    public void Clause_ValidDirective_AddsAllSortCriteria()
    {
        var criteria = new SortCriteria<Product>();
        var builder = new SortCriteriaBuilder<Product>(criteria);
        var parser = new SortDirectiveParser<Product>();

        builder.Clause("Name DESC, Price ASC", parser);

        Assert.Collection(criteria,
            c =>
            {
                Assert.Equal("Name", ((ISortCriterion)c).Property);
                Assert.Equal(SortDirection.Descending, c.Direction);
            },
            c =>
            {
                Assert.Equal("Price", ((ISortCriterion)c).Property);
                Assert.Equal(SortDirection.Ascending, c.Direction);
            });
    }

    [Fact]
    public void Clause_WithoutParser_UsesDefaultParser()
    {
        var criteria = new SortCriteria<Product>();
        var builder = new SortCriteriaBuilder<Product>(criteria);

        builder.Clause("Name ASC, Price DESC");

        Assert.Collection(criteria,
            c =>
            {
                Assert.Equal("Name", ((ISortCriterion)c).Property);
                Assert.Equal(SortDirection.Ascending, c.Direction);
            },
            c =>
            {
                Assert.Equal("Price", ((ISortCriterion)c).Property);
                Assert.Equal(SortDirection.Descending, c.Direction);
            });
    }

    [Fact]
    public void Clause_NullParser_ThrowsArgumentNullException()
    {
        var criteria = new SortCriteria<Product>();
        var builder = new SortCriteriaBuilder<Product>(criteria);

        Assert.Throws<ArgumentNullException>("parser", () => builder.Clause("Name DESC", parser: null!));
    }
}
