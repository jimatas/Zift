using Microsoft.EntityFrameworkCore;
using Zift.Pagination.Cursor;

namespace Zift.Pagination.Tests.Sqlite;

public sealed class IncludeAndTrackingTests : IAsyncDisposable
{
    private readonly SqliteFixture _fixture = new();
    public ValueTask DisposeAsync() => _fixture.DisposeAsync();

    private TestDbContext Db => _fixture.Context;

    [Fact]
    public async Task ReferenceInclude_LoadsNavigation_LikePlainEf_InBothDirections()
    {
        var expected = await Db.Products.AsNoTracking().Include(p => p.Category).OrderBy(p => p.Id)
            .Select(p => new { p.Id, CategoryId = p.Category == null ? (int?)null : p.Category.Id })
            .ToListAsync();
        var order = Db.Products.AsNoTracking().Include(p => p.Category).AsCursorQuery().OrderBy(p => p.Id);

        var forward = new List<(int Id, int? CategoryId)>();
        var page = await order.ToCursorPageAsync(5);
        while (true)
        {
            forward.AddRange(page.Items.Select(p => (p.Id, p.Category?.Id)));
            if (!page.HasNextPage) break;
            page = await order.After(page.EndCursor).ToCursorPageAsync(5);
        }

        var backward = new List<(int Id, int? CategoryId)>();
        while (true)
        {
            backward.InsertRange(0, page.Items.Select(p => (p.Id, p.Category?.Id)));
            if (!page.HasPreviousPage) break;
            page = await order.Before(page.StartCursor).ToCursorPageAsync(5);
        }

        var expectedPairs = expected.Select(p => (p.Id, p.CategoryId)).ToList();
        Assert.Contains(expectedPairs, p => p.CategoryId is not null);
        Assert.Contains(expectedPairs, p => p.CategoryId is null);
        Assert.Equal(expectedPairs, forward);
        Assert.Equal(expectedPairs, backward);
    }

    [Fact]
    public async Task WithoutInclude_NavigationIsNotLoaded()
    {
        var page = await Db.Products.AsNoTracking().AsCursorQuery().OrderBy(p => p.Id).ToCursorPageAsync(23);

        Assert.All(page.Items, p => Assert.Null(p.Category));
    }

    [Fact]
    public async Task TrackingQuery_TracksReturnedItems_AndTheLookAheadRow()
    {
        var page = await Db.Products.AsCursorQuery().OrderBy(p => p.Id).ToCursorPageAsync(5);

        Assert.True(page.HasNextPage);
        Assert.All(page.Items, p => Assert.Equal(EntityState.Unchanged, Db.Entry(p).State));
        Assert.Equal(page.Items.Count + 1, Db.ChangeTracker.Entries<Product>().Count());
    }

    [Fact]
    public async Task TrackingQuery_OnLastPage_TracksOnlyReturnedItems()
    {
        var page = await Db.Products.AsCursorQuery().OrderBy(p => p.Id).ToCursorPageAsync(100);

        Assert.False(page.HasNextPage);
        Assert.Equal(page.Items.Count, Db.ChangeTracker.Entries<Product>().Count());
    }

    [Fact]
    public async Task TrackingQuery_WithInclude_TracksIncludedEntities()
    {
        var page = await Db.Products.Include(p => p.Category).AsCursorQuery().OrderBy(p => p.Id).ToCursorPageAsync(23);

        var loaded = page.Items.Where(p => p.Category is not null).Select(p => p.Category!.Id).Distinct().Count();
        Assert.True(loaded > 1);
        Assert.Equal(loaded, Db.ChangeTracker.Entries<Category>().Count(e => page.Items.Any(p => p.Category == e.Entity)));
    }

    [Fact]
    public async Task AsNoTracking_TracksNothing_AndStillHonorsInclude()
    {
        var page = await Db.Products.AsNoTracking().Include(p => p.Category)
            .AsCursorQuery().OrderBy(p => p.Id).ToCursorPageAsync(10);

        Assert.Empty(Db.ChangeTracker.Entries());
        Assert.Contains(page.Items, p => p.Category is not null);
    }

    [Fact]
    public async Task AsNoTrackingWithIdentityResolution_SharesIncludedInstances_WithoutTracking()
    {
        var page = await Db.Products.AsNoTrackingWithIdentityResolution().Include(p => p.Category)
            .AsCursorQuery().OrderBy(p => p.Id).ToCursorPageAsync(23);

        Assert.Empty(Db.ChangeTracker.Entries());
        var groups = page.Items.Where(p => p.Category is not null).GroupBy(p => p.Category!.Id).ToList();
        Assert.Contains(groups, g => g.Count() > 1);
        Assert.All(groups, g => Assert.Single(g.Select(p => p.Category).Distinct()));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CollectionInclude_LoadsChildren_ForEveryPage_SingleAndSplitQuery(bool splitQuery)
    {
        var expected = (await Db.Categories.AsNoTracking().Include(c => c.Children).OrderBy(c => c.Id).ToListAsync())
            .Select(c => (c.Id, Children: string.Join(",", c.Children.Select(x => x.Id).Order())))
            .ToList();
        Assert.Contains(expected, c => c.Children.Contains(','));
        Assert.Contains(expected, c => c.Children.Length == 0);

        IQueryable<Category> source = Db.Categories.AsNoTracking().Include(c => c.Children);
        if (splitQuery)
        {
            source = source.AsSplitQuery();
        }

        var order = source.AsCursorQuery().OrderBy(c => c.Id);

        var forward = new List<(int Id, string Children)>();
        var page = await order.ToCursorPageAsync(1);
        while (true)
        {
            forward.AddRange(page.Items.Select(c => (c.Id, string.Join(",", c.Children.Select(x => x.Id).Order()))));
            if (!page.HasNextPage) break;
            page = await order.After(page.EndCursor).ToCursorPageAsync(1);
        }

        var backward = new List<(int Id, string Children)>();
        while (true)
        {
            backward.InsertRange(0, page.Items.Select(c => (c.Id, string.Join(",", c.Children.Select(x => x.Id).Order()))));
            if (!page.HasPreviousPage) break;
            page = await order.Before(page.StartCursor).ToCursorPageAsync(1);
        }

        Assert.Equal(expected, forward);
        Assert.Equal(expected, backward);
    }

    [Fact]
    public async Task CollectionInclude_WithPageLargerThanOne_KeepsAllChildrenOfEachItem()
    {
        var page = await Db.Categories.AsNoTracking().Include(c => c.Children)
            .AsSplitQuery().AsCursorQuery().OrderBy(c => c.Id).ToCursorPageAsync(2);

        var books = Assert.Single(page.Items, c => c.Title == "Books");
        Assert.Equal(["Games", "Software"], books.Children.Select(c => c.Title).Order());
        Assert.True(page.HasNextPage);
    }
}
