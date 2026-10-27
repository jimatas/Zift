using Microsoft.EntityFrameworkCore;
using Zift.Pagination.Cursor;

namespace Zift.Pagination.Hardening;

public sealed class OptionalNavigationTests : IAsyncDisposable
{
    private readonly SqliteFixture _fixture = new();
    public ValueTask DisposeAsync() => _fixture.DisposeAsync();

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task ParsedValueLeaf_TraversesBothWays_AcrossMissingCategories(bool descending, bool nullGuards)
    {
        var source = _fixture.Context.Products.AsNoTracking();
        var expected = await (descending
            ? source.OrderByDescending(p => (int?)p.Category!.Id).ThenBy(p => p.Id)
            : source.OrderBy(p => (int?)p.Category!.Id).ThenBy(p => p.Id))
            .Select(p => p.Id).ToListAsync();
        var order = source.AsCursorQuery().OrderBy(descending ? "Category.Id DESC, Id" : "Category.Id, Id",
            new OrderingOptions { EnableNullGuards = nullGuards });
        await AssertTraversalAsync(order, expected);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TypedExplicitNullableLeaf_TraversesBothWays(bool descending)
    {
        var source = _fixture.Context.Products.AsNoTracking();
        var expected = await (descending
            ? source.OrderByDescending(p => (int?)p.Category!.Id).ThenBy(p => p.Id)
            : source.OrderBy(p => (int?)p.Category!.Id).ThenBy(p => p.Id))
            .Select(p => p.Id).ToListAsync();
        var order = descending
            ? source.AsCursorQuery().OrderByDescending(p => (int?)p.Category!.Id).ThenBy(p => p.Id)
            : source.AsCursorQuery().OrderBy(p => (int?)p.Category!.Id).ThenBy(p => p.Id);
        await AssertTraversalAsync(order, expected);

        var first = await order.ToCursorPageAsync(1);
        var parsedNext = await source.AsCursorQuery().OrderBy(descending ? "Category.Id DESC, Id" : "Category.Id, Id")
            .After(first.EndCursor).ToCursorPageAsync(1);
        var typedNext = await order.After(first.EndCursor).ToCursorPageAsync(1);
        Assert.Equal(typedNext.Items.Select(p => p.Id), parsedNext.Items.Select(p => p.Id));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NullableBooleanSelector_TraversesBothWays_AcrossNullFalseAndTrue(bool descending)
    {
        var source = _fixture.Context.Products.AsNoTracking();
        var expected = await (descending
            ? source.OrderByDescending(p => p.Category == null ? (bool?)null : p.Flag).ThenBy(p => p.Id)
            : source.OrderBy(p => p.Category == null ? (bool?)null : p.Flag).ThenBy(p => p.Id))
            .Select(p => p.Id).ToListAsync();
        var order = descending
            ? source.AsCursorQuery().OrderByDescending(p => p.Category == null ? (bool?)null : p.Flag).ThenBy(p => p.Id)
            : source.AsCursorQuery().OrderBy(p => p.Category == null ? (bool?)null : p.Flag).ThenBy(p => p.Id);
        await AssertTraversalAsync(order, expected);
    }

    internal static async Task AssertTraversalAsync(IOrderedCursorQuery<Product> order, List<int> expected)
    {
        var forward = new List<int>();
        var page = await order.ToCursorPageAsync(1);
        var firstCursor = page.StartCursor;
        while (true)
        {
            forward.AddRange(page.Items.Select(p => p.Id));
            Assert.True(forward.Count <= expected.Count, "Traversal did not terminate.");
            if (!page.HasNextPage) break;
            page = await order.After(page.EndCursor).ToCursorPageAsync(1);
        }
        Assert.Equal(expected, forward);
        Assert.Empty((await order.After(page.EndCursor).ToCursorPageAsync(1)).Items);

        var backward = new List<int>();
        while (true)
        {
            backward.InsertRange(0, page.Items.Select(p => p.Id));
            Assert.True(backward.Count <= expected.Count, "Traversal did not terminate.");
            if (!page.HasPreviousPage) break;
            page = await order.Before(page.StartCursor).ToCursorPageAsync(1);
        }
        Assert.Equal(expected, backward);
        Assert.Empty((await order.Before(firstCursor).ToCursorPageAsync(1)).Items);
    }
}
