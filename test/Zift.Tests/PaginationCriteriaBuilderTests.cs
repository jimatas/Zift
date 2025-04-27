namespace Zift.Tests;

using Pagination;
using SharedFixture.Models;

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
}
