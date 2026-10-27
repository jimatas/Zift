using Zift.Filtering.ExpressionBuilding;
using Microsoft.EntityFrameworkCore;
using Zift.Fixture;

namespace Zift.Filtering;

public sealed class NullPropagationSqliteTests
{
    private static readonly ExpressionBuilderOptions GuardsOn = new() { EnableNullGuards = true };
    private static readonly ExpressionBuilderOptions GuardsOff = new() { EnableNullGuards = false };

    public static IEnumerable<object[]> ReviewFilters =>
    [
        ["Rating == null"],
        ["Rating != null"],
        ["Rating == 5"],
        ["Rating != 5"],
        ["Rating > 3"],
        ["Rating in [4, 5]"],
        ["Rating in [4, null]"],
        ["Rating in []"],
        ["Author.Name == 'John Doe'"],
        ["Author.Name != 'John Doe'"],
        ["Author.Name == null"],
        ["Author.Name != null"],
        ["Author.Name %= 'o'"],
        ["Author.Name ^= 'J'"],
        ["Author.Name > 'B'"],
        ["Author.Name in ['John Doe', 'Jane Smith']"],
        ["Author.Name in ['John Doe', null]"],
        ["!(Author.Name == 'John Doe')"],
        ["Author.Name != 'John Doe' && Rating != 5"]
    ];

    public static IEnumerable<object[]> ProductFilters =>
    [
        ["Reviews:count == 0"],
        ["Reviews:count != 1"],
        ["Reviews:count > 1"],
        ["Reviews:any()"],
        ["Reviews:any(Rating == null)"],
        ["Reviews:any(Rating != 5)"],
        ["Reviews:any(Author.Name %= 'o')"],
        ["Reviews:any(Author.Name == null)"],
        ["Reviews:any(Author.Name != 'John Doe')"],
#if !NET8_0
        ["Reviews:all(Rating >= 4)"],
#endif
        ["Reviews:all(Rating != 5)"],
        ["Reviews:all(Author.Name != 'John Doe')"],
        ["!Reviews:any(Author.Name == null)"]
    ];

    [Theory]
    [MemberData(nameof(ReviewFilters))]
    public async Task Reviews_NullGuardsOn_TranslateAndMatchNativeSemantics(string filter)
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var native = await fixture.Context.Reviews
            .Where(filter, GuardsOff)
            .Select(r => r.Id)
            .ToListAsync();

        var guarded = await fixture.Context.Reviews
            .Where(filter, GuardsOn)
            .Select(r => r.Id)
            .ToListAsync();

        Assert.Equal(native.Order(), guarded.Order());
    }

    [Theory]
    [MemberData(nameof(ReviewFilters))]
    public async Task Reviews_InMemoryWithNullGuards_MatchesNativeSemantics(string filter)
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var native = await fixture.Context.Reviews
            .Where(filter, GuardsOff)
            .Select(r => r.Id)
            .ToListAsync();

        var loaded = await fixture.Context.Reviews
            .Include(r => r.Author)
            .AsNoTracking()
            .ToListAsync();

        var inMemory = loaded
            .AsQueryable()
            .Where(filter, GuardsOn)
            .Select(r => r.Id)
            .ToList();

        Assert.Equal(native.Order(), inMemory.Order());
    }

    [Theory]
    [MemberData(nameof(ProductFilters))]
    public async Task Products_NullGuardsOn_TranslateAndMatchNativeSemantics(string filter)
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var native = await fixture.Context.Products
            .Where(filter, GuardsOff)
            .Select(p => p.Id)
            .ToListAsync();

        var guarded = await fixture.Context.Products
            .Where(filter, GuardsOn)
            .Select(p => p.Id)
            .ToListAsync();

        Assert.Equal(native.Order(), guarded.Order());
    }

    [Theory]
    [MemberData(nameof(ProductFilters))]
    public async Task Products_InMemoryWithNullGuards_MatchesNativeSemantics(string filter)
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var native = await fixture.Context.Products
            .Where(filter, GuardsOff)
            .Select(p => p.Id)
            .ToListAsync();

        var loaded = await fixture.Context.Products
            .Include(p => p.Reviews)
            .ThenInclude(r => r.Author)
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
    public async Task Products_AllOverNullableRating_InMemoryTreatsNullRatingAsFailing()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var loaded = await fixture.Context.Products
            .Include(p => p.Reviews)
            .AsNoTracking()
            .ToListAsync();

        var ids = loaded
            .AsQueryable()
            .Where("Reviews:all(Rating >= 4)", GuardsOn)
            .Select(p => p.Id)
            .ToList();

        Assert.Equal(
            new[] { CatalogFixture.Ids.Products.Smartphone, CatalogFixture.Ids.Products.GreatGatsby }.Order(),
            ids.Order());
    }

    [Fact]
    public async Task Products_AllOverNullableRating_OnDatabase_DependsOnEfCoreVersion()
    {
        await using var fixture = new SqliteTestFixture();
        await fixture.SeedAsync(CatalogFixture.Create());

        var ids = await fixture.Context.Products
            .Where("Reviews:all(Rating >= 4)", GuardsOn)
            .Select(p => p.Id)
            .ToListAsync();

#if NET8_0
        Guid[] expected =
        [
            CatalogFixture.Ids.Products.Smartphone,
            CatalogFixture.Ids.Products.GreatGatsby,
            CatalogFixture.Ids.Products.NineteenEightyFour
        ];
#else
        Guid[] expected =
        [
            CatalogFixture.Ids.Products.Smartphone,
            CatalogFixture.Ids.Products.GreatGatsby
        ];
#endif

        Assert.Equal(expected.Order(), ids.Order());
    }
}
