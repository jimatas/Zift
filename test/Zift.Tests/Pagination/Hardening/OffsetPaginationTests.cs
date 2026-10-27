using Zift.Pagination.Offset;

namespace Zift.Pagination.Hardening;

public class OffsetPaginationTests
{
    private static IQueryable<int> Numbers(int count)
    {
        return Enumerable.Range(1, count).AsQueryable();
    }

    [Fact]
    public void ToPage_ReturnsRequestedSlice_WithMetadata()
    {
        var page = Numbers(10).ToPage(pageNumber: 2, pageSize: 3);

        Assert.Equal([4, 5, 6], page.Items);
        Assert.Equal(2, page.PageNumber);
        Assert.Equal(4, page.PageCount);
        Assert.Equal(10, page.TotalItemCount);
        Assert.True(page.HasNextPage);
        Assert.True(page.HasPreviousPage);
    }

    [Fact]
    public void Empty_IsFirstPageWithoutItemsOrNeighbours()
    {
        var page = Page<int>.Empty(pageSize: 5);

        Assert.Empty(page.Items);
        Assert.Equal(1, page.PageNumber);
        Assert.Equal(5, page.PageSize);
        Assert.Equal(0, page.PageCount);
        Assert.Equal(0, page.TotalItemCount);
        Assert.False(page.HasNextPage);
        Assert.False(page.HasPreviousPage);
    }

    [Fact]
    public void Empty_WithInvalidPageSize_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Page<int>.Empty(0));
    }

    [Fact]
    public void ToPage_LastPage_HasNoNextPage()
    {
        var page = Numbers(10).ToPage(4, 3);

        Assert.Equal([10], page.Items);
        Assert.False(page.HasNextPage);
    }

    [Fact]
    public void ToPage_OffsetBeyondIntRange_Throws_InsteadOfWrappingAround()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Numbers(3).ToPage(1_073_741_825, 4));
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    public void ToPage_InvalidArguments_Throw(int pageNumber, int pageSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Numbers(3).ToPage(pageNumber, pageSize));
    }

    [Fact]
    public void Map_ProjectsItems_AndKeepsMetadata()
    {
        var page = Numbers(10).ToPage(2, 3).Map(n => n * 10);

        Assert.Equal([40, 50, 60], page.Items);
        Assert.Equal(10, page.TotalItemCount);
    }

    [Fact]
    public void Page_ItemsExceedingPageSize_AreRejected()
    {
        var error = Assert.Throws<ArgumentException>(() => new Page<int>([1, 2, 3], 1, 2, 10));
        Assert.Equal("items", error.ParamName);
    }

    [Fact]
    public void Page_AllowsSkewedCountsFromConcurrentChanges()
    {
        var page = new Page<int>([1, 2], pageNumber: 1, pageSize: 5, totalItemCount: 1);
        Assert.Equal(2, page.Items.Count);

        var emptyPastEnd = new Page<int>([], pageNumber: 9, pageSize: 5, totalItemCount: 3);
        Assert.Empty(emptyPastEnd.Items);
    }
}
