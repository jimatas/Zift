namespace Zift.Tests;

using Pagination;
using SharedFixture.Models;

public class PaginatedListExtensionsTests
{
    [Fact]
    public void IsFirstPage_WhenPageNumberIsOne_ReturnsTrue()
    {
        var list =  CreatePaginatedList(pageNumber: 1, pageCount: 5);

        Assert.True(list.IsFirstPage());
    }

    [Fact]
    public void IsFirstPage_WhenPageNumberIsGreaterThanOne_ReturnsFalse()
    {
        var list = CreatePaginatedList(pageNumber: 2, pageCount: 5);

        Assert.False(list.IsFirstPage());
    }

    [Fact]
    public void IsLastPage_WhenPageNumberEqualsPageCount_ReturnsTrue()
    {
        var list = CreatePaginatedList(pageNumber: 3, pageCount: 3);

        Assert.True(list.IsLastPage());
    }

    [Fact]
    public void IsLastPage_WhenPageNumberIsLessThanPageCount_ReturnsFalse()
    {
        var list = CreatePaginatedList(pageNumber: 2, pageCount: 3);

        Assert.False(list.IsLastPage());
    }

    [Fact]
    public void HasNextPage_WhenPageNumberIsLessThanPageCount_ReturnsTrue()
    {
        var list = CreatePaginatedList(pageNumber: 2, pageCount: 3);

        Assert.True(list.HasNextPage());
    }

    [Fact]
    public void HasNextPage_WhenPageNumberEqualsPageCount_ReturnsFalse()
    {
        var list = CreatePaginatedList(pageNumber: 3, pageCount: 3);

        Assert.False(list.HasNextPage());
    }

    [Fact]
    public void HasPreviousPage_WhenPageNumberIsGreaterThanOne_ReturnsTrue()
    {
        var list = CreatePaginatedList(pageNumber: 2, pageCount: 5);

        Assert.True(list.HasPreviousPage());
    }

    [Fact]
    public void HasPreviousPage_WhenPageNumberIsOne_ReturnsFalse()
    {
        var list = CreatePaginatedList(pageNumber: 1, pageCount: 5);

        Assert.False(list.HasPreviousPage());
    }

    #region Fixture
    private static PaginatedList<Product> CreatePaginatedList(int pageNumber, int pageCount)
    {
        var pageSize = 10;
        var totalCount = pageCount * pageSize;

        var items = Enumerable.Range(0, pageSize)
            .Select(i => new Product { Name = $"Product {i + 1}" })
            .ToArray();

        return new(pageNumber, pageSize, items, totalCount);
    }
    #endregion
}
