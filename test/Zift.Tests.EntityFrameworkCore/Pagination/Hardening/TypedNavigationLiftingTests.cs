using Microsoft.EntityFrameworkCore;
using Zift.Pagination.Cursor;

namespace Zift.Pagination.Tests.Sqlite;

public sealed class TypedNavigationLiftingTests : IAsyncDisposable
{
    private readonly SqliteFixture _fixture = new();
    public ValueTask DisposeAsync() => _fixture.DisposeAsync();

    private IQueryable<Product> Products => _fixture.Context.Products.AsNoTracking();

    public static TheoryData<LiftCase, bool> Cases()
    {
        var data = new TheoryData<LiftCase, bool>();
        foreach (var liftCase in LiftCase.All)
        {
            data.Add(liftCase, false);
            data.Add(liftCase, true);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task TypedSelector_TraversesBothWays_AcrossMissingNegativeZeroAndPositiveLeaves(
        LiftCase liftCase, bool descending)
    {
        var expected = await liftCase.ExpectedAsync(Products, descending);

        await OptionalNavigationTests.AssertTraversalAsync(liftCase.Typed(Products, descending), expected);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task ExplicitNullableSelector_TraversesBothWays_AsBaseline(LiftCase liftCase, bool descending)
    {
        var expected = await liftCase.ExpectedAsync(Products, descending);

        await OptionalNavigationTests.AssertTraversalAsync(liftCase.Explicit(Products, descending), expected);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task UnguardedStringPath_TraversesBothWays_AsBaseline(LiftCase liftCase, bool descending)
    {
        var expected = await liftCase.ExpectedAsync(Products, descending);

        await OptionalNavigationTests.AssertTraversalAsync(liftCase.Parsed(Products, descending), expected);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task TypedExplicitAndParsedSelectors_ProduceCompatibleCursors_InBothDirections(
        LiftCase liftCase, bool descending)
    {
        var builders = new (string Name, Func<IOrderedCursorQuery<Product>> Build)[]
        {
            ("typed", () => liftCase.Typed(Products, descending)),
            ("explicit", () => liftCase.Explicit(Products, descending)),
            ("parsed", () => liftCase.Parsed(Products, descending))
        };

        foreach (var source in builders)
        {
            var sourceQuery = source.Build();
            var first = await sourceQuery.ToCursorPageAsync(3);
            var middle = await sourceQuery.After(first.EndCursor).ToCursorPageAsync(3);
            var forward = await sourceQuery.After(middle.EndCursor).ToCursorPageAsync(3);
            var backward = await sourceQuery.Before(middle.StartCursor).ToCursorPageAsync(3);

            foreach (var target in builders)
            {
                var targetQuery = target.Build();
                var context = $"{source.Name} cursor used by {target.Name}";

                var forwardFromTarget = await targetQuery.After(middle.EndCursor).ToCursorPageAsync(3);
                var backwardFromTarget = await targetQuery.Before(middle.StartCursor).ToCursorPageAsync(3);

                Assert.True(
                    forward.Items.Select(p => p.Id).SequenceEqual(forwardFromTarget.Items.Select(p => p.Id)),
                    context + " (forward)");
                Assert.True(
                    backward.Items.Select(p => p.Id).SequenceEqual(backwardFromTarget.Items.Select(p => p.Id)),
                    context + " (backward)");
                Assert.Equal(forward.EndCursor, forwardFromTarget.EndCursor);
                Assert.Equal(backward.StartCursor, backwardFromTarget.StartCursor);
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TypedLiftedKey_AsSecondaryKey_AfterTies_TraversesBothWays(bool descending)
    {
        var source = Products;
        var expected = await (descending
            ? source.OrderBy(p => p.Name).ThenByDescending(p => (int?)p.Category!.Rank).ThenBy(p => p.Id)
            : source.OrderBy(p => p.Name).ThenBy(p => (int?)p.Category!.Rank).ThenBy(p => p.Id))
            .Select(p => p.Id).ToListAsync();
        var order = descending
            ? source.AsCursorQuery().OrderBy(p => p.Name).ThenByDescending(p => p.Category!.Rank).ThenBy(p => p.Id)
            : source.AsCursorQuery().OrderBy(p => p.Name).ThenBy(p => p.Category!.Rank).ThenBy(p => p.Id);

        await OptionalNavigationTests.AssertTraversalAsync(order, expected);
    }

    [Fact]
    public async Task TypedLiftedKeys_AcrossSeveralNavigationLevels_AndKeys_TraverseBothWays()
    {
        var source = Products;
        var expected = await source
            .OrderByDescending(p => (int?)p.Category!.Parent!.Rank)
            .ThenBy(p => (bool?)p.Category!.IsActive)
            .ThenBy(p => p.Id)
            .Select(p => p.Id).ToListAsync();
        var order = source.AsCursorQuery()
            .OrderByDescending(p => p.Category!.Parent!.Rank)
            .ThenBy(p => p.Category!.IsActive)
            .ThenBy(p => p.Id);

        await OptionalNavigationTests.AssertTraversalAsync(order, expected);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NavigationDeclaredNonNullable_OverOptionalRelationship_IsStillLifted(bool descending)
    {
        var source = Products;
        Assert.Contains(await source.Select(p => p.AlternateCategoryId).ToListAsync(), id => id is null);
        var expected = await (descending
            ? source.OrderByDescending(p => (int?)p.AlternateCategory.Rank).ThenBy(p => p.Id)
            : source.OrderBy(p => (int?)p.AlternateCategory.Rank).ThenBy(p => p.Id))
            .Select(p => p.Id).ToListAsync();
        var order = descending
            ? source.AsCursorQuery().OrderByDescending(p => p.AlternateCategory.Rank).ThenBy(p => p.Id)
            : source.AsCursorQuery().OrderBy(p => p.AlternateCategory.Rank).ThenBy(p => p.Id);

        await OptionalNavigationTests.AssertTraversalAsync(order, expected);
    }

    [Fact]
    public async Task RootValueProperties_StillPageNormally()
    {
        var expected = await Products.OrderBy(p => p.Price).ThenBy(p => p.Id).Select(p => p.Id).ToListAsync();

        await OptionalNavigationTests.AssertTraversalAsync(
            Products.AsCursorQuery().OrderBy(p => p.Price).ThenBy(p => p.Id), expected);
    }

    public abstract class LiftCase
    {
        public static IReadOnlyList<LiftCase> All { get; } =
        [
            new LiftCase<int, int?>("Category.Id", p => p.Category!.Id, p => (int?)p.Category!.Id),
            new LiftCase<int, int?>("Category.Rank", p => p.Category!.Rank, p => (int?)p.Category!.Rank),
            new LiftCase<int, int?>(
                "Category.Parent.Rank", p => p.Category!.Parent!.Rank, p => (int?)p.Category!.Parent!.Rank),
            new LiftCase<bool, bool?>(
                "Category.IsActive", p => p.Category!.IsActive, p => (bool?)p.Category!.IsActive)
        ];

        public abstract IOrderedCursorQuery<Product> Typed(IQueryable<Product> source, bool descending);

        public abstract IOrderedCursorQuery<Product> Explicit(IQueryable<Product> source, bool descending);

        public abstract IOrderedCursorQuery<Product> Parsed(IQueryable<Product> source, bool descending);

        public abstract Task<List<int>> ExpectedAsync(IQueryable<Product> source, bool descending);
    }

    public sealed class LiftCase<TValue, TNullable>(
        string path,
        Expression<Func<Product, TValue>> typed,
        Expression<Func<Product, TNullable>> explicitNullable) : LiftCase
    {
        public override IOrderedCursorQuery<Product> Typed(IQueryable<Product> source, bool descending)
        {
            return Order(source, typed, descending);
        }

        public override IOrderedCursorQuery<Product> Explicit(IQueryable<Product> source, bool descending)
        {
            return Order(source, explicitNullable, descending);
        }

        public override IOrderedCursorQuery<Product> Parsed(IQueryable<Product> source, bool descending)
        {
            return source.AsCursorQuery().OrderBy(descending ? $"{path} DESC, Id" : $"{path}, Id");
        }

        public override Task<List<int>> ExpectedAsync(IQueryable<Product> source, bool descending)
        {
            return (descending
                ? source.OrderByDescending(explicitNullable).ThenBy(p => p.Id)
                : source.OrderBy(explicitNullable).ThenBy(p => p.Id))
                .Select(p => p.Id).ToListAsync();
        }

        public override string ToString() => path;

        private static IOrderedCursorQuery<Product> Order<TKey>(
            IQueryable<Product> source, Expression<Func<Product, TKey>> key, bool descending)
        {
            return descending
                ? source.AsCursorQuery().OrderByDescending(key).ThenBy(p => p.Id)
                : source.AsCursorQuery().OrderBy(key).ThenBy(p => p.Id);
        }
    }
}
