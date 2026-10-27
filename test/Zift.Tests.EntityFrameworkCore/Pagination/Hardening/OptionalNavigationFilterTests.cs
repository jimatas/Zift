using Microsoft.EntityFrameworkCore;
using Zift.Filtering;
using Zift.Filtering.ExpressionBuilding;

namespace Zift.Pagination.Hardening;

public sealed class OptionalNavigationFilterTests : IAsyncDisposable
{
    private static readonly ExpressionBuilderOptions GuardsOn = new() { EnableNullGuards = true };
    private static readonly ExpressionBuilderOptions GuardsOff = new() { EnableNullGuards = false };

    private readonly SqliteFixture _fixture = new();

    public ValueTask DisposeAsync() => _fixture.DisposeAsync();

    public static IEnumerable<object[]> ProductFilters =>
    [
        ["Score == null"],
        ["Score != 3"],
        ["Score in [1, null]"],
        ["Category.Title == 'Books'"],
        ["Category.Title != 'Books'"],
        ["Category.Title == null"],
        ["Category.Title != null"],
        ["Category.Title %= 'o'"],
        ["Category.Title in ['Books', null]"],
        ["!(Category.Title == 'Books')"],
        ["Category.Rank > 0"],
        ["Category.Rank != 7"],
        ["Category.Rank in [0, 7]"],
        ["Category.Parent.Title != 'Books'"],
        ["Category.Parent.Title == null"],
        ["Category.Parent.Title %= 'o'"],
        ["Category.Children:count == 0"],
        ["Category.Children:any(Title == 'Games')"],
        ["Category.Children:all(IsActive == true)"]
    ];

    [Theory]
    [MemberData(nameof(ProductFilters))]
    public async Task Products_NullGuardsOn_TranslateAndMatchNativeSemantics(string filter)
    {
        var native = await _fixture.Context.Products
            .Where(filter, GuardsOff)
            .Select(p => p.Id)
            .ToListAsync();

        var guarded = await _fixture.Context.Products
            .Where(filter, GuardsOn)
            .Select(p => p.Id)
            .ToListAsync();

        Assert.NotEmpty(native.Union(guarded));
        Assert.Equal(native.Order(), guarded.Order());
    }

    [Theory]
    [MemberData(nameof(ProductFilters))]
    public async Task Products_InMemoryWithNullGuards_MatchesNativeSemantics(string filter)
    {
        var native = await _fixture.Context.Products
            .Where(filter, GuardsOff)
            .Select(p => p.Id)
            .ToListAsync();

        var loaded = await _fixture.Context.Products
            .Include(p => p.Category!)
            .ThenInclude(c => c.Parent)
            .Include(p => p.Category!)
            .ThenInclude(c => c.Children)
            .AsNoTracking()
            .ToListAsync();

        var inMemory = loaded
            .AsQueryable()
            .Where(filter, GuardsOn)
            .Select(p => p.Id)
            .ToList();

        Assert.Equal(native.Order(), inMemory.Order());
    }

    [Fact]
    public async Task Products_ValueTypeLeafBehindOptionalNavigation_CanBeComparedWithNull()
    {
        var expected = await _fixture.Context.Products
            .Where(p => p.Category == null)
            .Select(p => p.Id)
            .ToListAsync();

        var onDatabase = await _fixture.Context.Products
            .Where("Category.Rank == null", GuardsOn)
            .Select(p => p.Id)
            .ToListAsync();

        var loaded = await _fixture.Context.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .ToListAsync();

        var inMemory = loaded
            .AsQueryable()
            .Where("Category.Rank == null", GuardsOn)
            .Select(p => p.Id)
            .ToList();

        Assert.NotEmpty(expected);
        Assert.Equal(expected.Order(), onDatabase.Order());
        Assert.Equal(expected.Order(), inMemory.Order());
    }

    [Fact]
    public async Task Products_ValueTypeLeafBehindOptionalNavigation_WithoutGuards_IsStillRejected()
    {
        await Assert.ThrowsAsync<InvalidFilterException>(async () =>
            await _fixture.Context.Products
                .Where("Category.Rank == null", GuardsOff)
                .Select(p => p.Id)
                .ToListAsync());
    }
}
