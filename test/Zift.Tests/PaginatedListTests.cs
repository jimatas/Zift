namespace Zift.Tests;

using Pagination;
using SharedFixture.Models;

public class PaginatedListTests
{
    [Fact]
    public void Empty_ReturnsEmptyList()
    {
        var paginatedList = PaginatedList<Product>.Empty;

        Assert.Empty(paginatedList);
        Assert.Equal(0, paginatedList.TotalCount);
        Assert.Equal(0, paginatedList.PageCount);
        Assert.Equal(1, paginatedList.PageNumber);
        Assert.Equal(PaginationCriteria<Product>.DefaultPageSize, paginatedList.PageSize);
    }

    [Fact]
    public void Constructor_ValidParameters_SetsExpectedValues()
    {
        var items = new[] { new Product { Name = "Product1" }, new Product { Name = "Product2" } };

        var paginatedList = new PaginatedList<Product>(pageNumber: 1, pageSize: 2, items, totalCount: 2);

        Assert.Equal(2, paginatedList.Count);
        Assert.Equal(1, paginatedList.PageNumber);
        Assert.Equal(2, paginatedList.PageSize);
        Assert.Equal(1, paginatedList.PageCount);
        Assert.Equal(2, paginatedList.TotalCount);
    }

    [Fact]
    public void Constructor_InvalidPageNumber_ThrowsArgumentOutOfRangeException()
    {
        var items = new[] { new Product { Name = "Product1" } };

        Assert.Throws<ArgumentOutOfRangeException>("pageNumber", () => new PaginatedList<Product>(pageNumber: 0, pageSize: 1, items, totalCount: 1));
    }

    [Fact]
    public void Constructor_InvalidPageSize_ThrowsArgumentOutOfRangeException()
    {
        var items = new[] { new Product { Name = "Product1" } };

        Assert.Throws<ArgumentOutOfRangeException>("pageSize", () => new PaginatedList<Product>(pageNumber: 1, pageSize: 0, items, totalCount: 1));
    }

    [Fact]
    public void Constructor_InvalidTotalCount_ThrowsArgumentOutOfRangeException()
    {
        var items = new[] { new Product { Name = "Product1" } };

        Assert.Throws<ArgumentOutOfRangeException>("totalCount", () => new PaginatedList<Product>(pageNumber: 1, pageSize: 1, items, totalCount: -1));
    }

    [Fact]
    public void Constructor_TotalCountLessThanItemsCount_ThrowsArgumentOutOfRangeException()
    {
        var items = new[] { new Product { Name = "Product1" }, new Product { Name = "Product2" } };

        Assert.Throws<ArgumentOutOfRangeException>("totalCount", () => new PaginatedList<Product>(pageNumber: 1, pageSize: 2, items, totalCount: 1));
    }

    [Fact]
    public void Constructor_NullItems_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("items", () => new PaginatedList<Product>(pageNumber: 1, pageSize: 1, items: null!, totalCount: 0));
    }

    [Theory]
    [InlineData(45, 10, 5)]
    [InlineData(46, 10, 5)]
    [InlineData(51, 10, 6)]
    [InlineData(0, 10, 0)]
    public void Constructor_CalculatesPageCountCorrectly(int totalCount, int pageSize, int expectedPageCount)
    {
        var items = Enumerable.Repeat(new Product(), Math.Min(pageSize, totalCount)).ToArray();

        var paginatedList = new PaginatedList<Product>(1, pageSize, items, totalCount);

        Assert.Equal(expectedPageCount, paginatedList.PageCount);
    }

    [Fact]
    public void Indexer_ReturnsExpectedItems()
    {
        var items = new[]
        {
            new Product { Name = "Product1" },
            new Product { Name = "Product2" },
        };

        var paginatedList = new PaginatedList<Product>(pageNumber: 1, pageSize: 2, items, totalCount: 2);

        Assert.Equal("Product1", paginatedList[0].Name);
        Assert.Equal("Product2", paginatedList[1].Name);
    }

    [Fact]
    public void GetEnumerator_ReturnsExpectedItems()
    {
        var items = new[]
        {
            new Product { Name = "Product1" },
            new Product { Name = "Product2" },
        };

        var paginatedList = new PaginatedList<Product>(pageNumber: 1, pageSize: 2, items, totalCount: 2);

        IEnumerable enumerable = paginatedList;
        var enumerator = enumerable.GetEnumerator();
        
        Assert.True(enumerator.MoveNext());
        Assert.Equal("Product1", (enumerator.Current as Product)?.Name);

        Assert.True(enumerator.MoveNext());
        Assert.Equal("Product2", (enumerator.Current as Product)?.Name);
    }
}
