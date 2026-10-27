using Microsoft.EntityFrameworkCore;
using Zift.Pagination.Cursor;

namespace Zift.Pagination.Hardening;

public sealed class CursorPaginationSqliteTests : IAsyncDisposable
{
    private readonly SqliteFixture _fixture = new();

    private IQueryable<Product> Products => _fixture.Context.Products.AsNoTracking();

    public ValueTask DisposeAsync()
    {
        return _fixture.DisposeAsync();
    }

    public static TheoryData<string, int> Orderings => new()
    {
        { "Id", 1 }, { "Id", 5 }, { "Id", 100 },
        { "Id DESC", 4 },
        { "Name, Id", 3 },
        { "Name DESC, Id DESC", 4 },
        { "Nick, Id", 3 },                    // nullable string, nulls first
        { "Nick DESC, Id", 3 },
        { "Score, Id", 4 },                   // nullable int
        { "Score DESC, Id DESC", 4 },
        { "Status, Id", 5 },                  // enum
        { "Status DESC, Name, Id", 5 },
        { "Flag, Id", 5 },                    // bool
        { "Flag DESC, Id", 5 },
        { "Created, Id", 4 },                 // DateTime
        { "Created DESC, Price, Id", 4 },
        { "Price, Id", 4 },                   // double
        { "Key", 4 },                         // Guid
        { "Score, Nick DESC, Status, Id DESC", 2 },
    };

    private async Task<List<int>> ExpectedIdsAsync(string ordering)
    {
        return await ApplyOrdering(Products, ordering).Select(p => p.Id).ToListAsync();
    }

    private static IQueryable<Product> ApplyOrdering(IQueryable<Product> query, string ordering)
    {
        IOrderedQueryable<Product>? ordered = null;

        foreach (var entry in ordering.Split(',', StringSplitOptions.TrimEntries))
        {
            var parts = entry.Split(' ');
            var descending = parts.Length > 1 && parts[1] == "DESC";
            var property = typeof(Product).GetProperty(parts[0])!;

            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(Product), "p");
            var selector = System.Linq.Expressions.Expression.Lambda(
                System.Linq.Expressions.Expression.Property(parameter, property), parameter);

            var method = (ordered, descending) switch
            {
                (null, false) => nameof(Queryable.OrderBy),
                (null, true) => nameof(Queryable.OrderByDescending),
                (_, false) => nameof(Queryable.ThenBy),
                (_, true) => nameof(Queryable.ThenByDescending)
            };

            var call = System.Linq.Expressions.Expression.Call(
                typeof(Queryable), method, [typeof(Product), property.PropertyType],
                (ordered ?? query).Expression, System.Linq.Expressions.Expression.Quote(selector));

            ordered = (IOrderedQueryable<Product>)query.Provider.CreateQuery<Product>(call);
        }

        return ordered!;
    }

    [Theory]
    [MemberData(nameof(Orderings))]
    public async Task ForwardTraversal_MatchesDatabaseOrdering(string ordering, int pageSize)
    {
        var expected = await ExpectedIdsAsync(ordering);
        var actual = new List<int>();
        string? cursor = null;

        do
        {
            var page = await Products.AsCursorQuery().OrderBy(ordering).After(cursor).ToCursorPageAsync(pageSize);
            actual.AddRange(page.Items.Select(p => p.Id));
            cursor = page.HasNextPage ? page.EndCursor : null;
            Assert.True(actual.Count <= expected.Count, "traversal returned more rows than exist");
        }
        while (cursor is not null);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(Orderings))]
    public async Task BackwardTraversal_MatchesDatabaseOrdering_InReverse(string ordering, int pageSize)
    {
        var expected = await ExpectedIdsAsync(ordering);

        var allPages = new List<List<int>>();
        string? cursor = null;
        CursorPage<Product> page;

        do
        {
            page = await Products.AsCursorQuery().OrderBy(ordering).After(cursor).ToCursorPageAsync(pageSize);
            allPages.Add(page.Items.Select(p => p.Id).ToList());
            cursor = page.HasNextPage ? page.EndCursor : null;
        }
        while (cursor is not null);

        var pagesBackward = new List<List<int>> { allPages[^1] };
        var back = page;

        while (back.HasPreviousPage)
        {
            back = await Products.AsCursorQuery().OrderBy(ordering).Before(back.StartCursor).ToCursorPageAsync(pageSize);
            pagesBackward.Add(back.Items.Select(p => p.Id).ToList());
        }

        pagesBackward.Reverse();
        Assert.Equal(expected, pagesBackward.SelectMany(ids => ids).ToList());
    }

    [Fact]
    public async Task SyncAndAsync_ReturnTheSamePage()
    {
        var first = Products.AsCursorQuery().OrderBy("Name, Id").ToCursorPage(5);
        var firstAsync = await Products.AsCursorQuery().OrderBy("Name, Id").ToCursorPageAsync(5);

        Assert.Equal(first.Items.Select(p => p.Id), firstAsync.Items.Select(p => p.Id));
        Assert.Equal(first.EndCursor, firstAsync.EndCursor);

        var second = Products.AsCursorQuery().OrderBy("Name, Id").After(first.EndCursor).ToCursorPage(5);
        var secondAsync = await Products.AsCursorQuery().OrderBy("Name, Id").After(first.EndCursor).ToCursorPageAsync(5);

        Assert.Equal(second.Items.Select(p => p.Id), secondAsync.Items.Select(p => p.Id));
    }

    [Fact]
    public async Task TypedAndParsedOrdering_ProduceInterchangeableCursors()
    {
        var typed = await Products.AsCursorQuery().OrderBy(p => p.Name).ThenByDescending(p => p.Id).ToCursorPageAsync(5);

        var parsed = await Products.AsCursorQuery().OrderBy("name, id desc").After(typed.EndCursor).ToCursorPageAsync(5);
        var typedNext = await Products.AsCursorQuery().OrderBy(p => p.Name).ThenByDescending(p => p.Id)
            .After(typed.EndCursor).ToCursorPageAsync(5);

        Assert.Equal(typedNext.Items.Select(p => p.Id), parsed.Items.Select(p => p.Id));
    }

    [Fact]
    public async Task NestedKey_OrdersAcrossTheNavigation_WithoutNullGuards()
    {
        var expected = await Products.OrderBy(p => p.Category!.Title).ThenBy(p => p.Id).Select(p => p.Id).ToListAsync();

        var actual = new List<int>();
        string? cursor = null;

        do
        {
            var page = await Products.AsCursorQuery().OrderBy("Category.Title, Id").After(cursor).ToCursorPageAsync(4);
            actual.AddRange(page.Items.Select(p => p.Id));
            cursor = page.HasNextPage ? page.EndCursor : null;
        }
        while (cursor is not null);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task FilteredQuery_PaginatesOnlyTheFilteredRows()
    {
        var expected = await Products.Where(p => p.Flag).OrderBy(p => p.Id).Select(p => p.Id).ToListAsync();

        var actual = new List<int>();
        string? cursor = null;

        do
        {
            var page = await Products.Where(p => p.Flag).AsCursorQuery().OrderBy(p => p.Id).After(cursor).ToCursorPageAsync(3);
            actual.AddRange(page.Items.Select(p => p.Id));
            cursor = page.HasNextPage ? page.EndCursor : null;
        }
        while (cursor is not null);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task ToPageAsync_ReturnsRequestedSlice()
    {
        var page = await Products.OrderBy(p => p.Id).ToPageAsync(pageNumber: 3, pageSize: 10);

        Assert.Equal([21, 22, 23], page.Items.Select(p => p.Id));
        Assert.Equal(23, page.TotalItemCount);
        Assert.Equal(3, page.PageCount);
        Assert.False(page.HasNextPage);
        Assert.True(page.HasPreviousPage);
    }

    [Fact]
    public async Task ToPageAsync_PageBeyondTheEnd_IsEmpty_ButKeepsTotals()
    {
        var page = await Products.OrderBy(p => p.Id).ToPageAsync(pageNumber: 9, pageSize: 10);

        Assert.Empty(page.Items);
        Assert.Equal(23, page.TotalItemCount);
    }
}
