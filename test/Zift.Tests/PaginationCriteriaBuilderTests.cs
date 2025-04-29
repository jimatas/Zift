namespace Zift.Tests;

using Pagination;
using SharedFixture.Models;

public class PaginationCriteriaBuilderTests
{
    [Fact]
    public void AtPage_SetsPageNumber()
    {
        var criteria = new PaginationCriteria<Product>();
        var builder = new PaginationCriteriaBuilder<Product>(criteria);

        builder.AtPage(3);

        Assert.Equal(3, criteria.PageNumber);
    }

    [Fact]
    public void WithSize_SetsPageSize()
    {
        var criteria = new PaginationCriteria<Product>();
        var builder = new PaginationCriteriaBuilder<Product>(criteria);

        builder.WithSize(50);

        Assert.Equal(50, criteria.PageSize);
    }
}
