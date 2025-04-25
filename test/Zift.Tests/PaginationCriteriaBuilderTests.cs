namespace Zift.Tests;

using Pagination;
using SharedFixture.Models;
using Sorting;
using Sorting.Dynamic;

public class PaginationCriteriaBuilderTests
{
    [Fact]
    public void StartAt_SetsPageNumber()
    {
        var criteria = new PaginationCriteria<Product>();
        var builder = new PaginationCriteriaBuilder<Product>(criteria);

        builder.StartAt(3);

        Assert.Equal(3, criteria.PageNumber);
    }

    [Fact]
    public void WithPageSize_SetsPageSize()
    {
        var criteria = new PaginationCriteria<Product>();
        var builder = new PaginationCriteriaBuilder<Product>(criteria);

        builder.WithPageSize(50);

        Assert.Equal(50, criteria.PageSize);
    }

    [Fact]
    public void SortBy_StringProperty_AddsSortCriterion()
    {
        var criteria = new PaginationCriteria<Product>();
        var builder = new PaginationCriteriaBuilder<Product>(criteria);

        builder.SortBy("Name", SortDirection.Descending);

        var criterion = Assert.Single(criteria.SortCriteria);
        Assert.Equal("Name", ((ISortCriterion)criterion).Property);
        Assert.Equal(SortDirection.Descending, criterion.Direction);
    }

    [Fact]
    public void SortBy_Expression_AddsSortCriterion()
    {
        var criteria = new PaginationCriteria<Product>();
        var builder = new PaginationCriteriaBuilder<Product>(criteria);

        builder.SortBy(p => p.Price, SortDirection.Ascending);

        var criterion = Assert.Single(criteria.SortCriteria);
        Assert.Equal("Price", ((ISortCriterion)criterion).Property);
        Assert.Equal(SortDirection.Ascending, criterion.Direction);
    }

    [Fact]
    public void SortBy_SortString_ParsesAndAddsAllCriteria()
    {
        var criteria = new PaginationCriteria<Product>();
        var builder = new PaginationCriteriaBuilder<Product>(criteria);
        var parser = new SortDirectiveParser<Product>();

        builder.SortBy("Name DESC, Price ASC", parser);

        Assert.Collection(criteria.SortCriteria,
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
    public void SortBy_SortStringWithNullParser_ThrowsArgumentNullException()
    {
        var criteria = new PaginationCriteria<Product>();
        var builder = new PaginationCriteriaBuilder<Product>(criteria);

        Assert.Throws<ArgumentNullException>("parser", () => builder.SortBy("Name DESC", parser: null!));
    }
}
