using Zift.Pagination.Cursor;

namespace Zift.Pagination.Hardening;

public class NullOrderingTests
{
    private sealed class Tag : IComparable<Tag>
    {
        public string Name { get; set; } = "";

        public int CompareTo(Tag? other) => other is null ? 1 : string.CompareOrdinal(Name, other.Name);
    }

    private sealed record Item(int Id, int? N, string? S, bool? B, Guid? G, Tag? T);

    private sealed class Leaf
    {
        public int Value { get; set; }
    }

    private sealed class Node
    {
        public int Id { get; set; }
        public Leaf? Leaf { get; set; }
    }

    private static Guid G(int n) => new(n, 0, 0, new byte[8]);

    private static Tag Tg(string name) => new() { Name = name };

    private static readonly Item[] Items =
    [
        new(1, 0, "b", false, G(1), Tg("a")),
        new(2, null, null, null, null, null),
        new(3, -1, "a", true, G(2), Tg("b")),
        new(4, 0, "b", null, G(1), null),
        new(5, null, "c", true, null, Tg("a")),
        new(6, 2, null, false, G(3), Tg("b")),
        new(7, -1, "a", null, null, null),
        new(8, null, null, true, G(2), Tg("a")),
        new(9, 2, "c", false, G(3), Tg("b")),
        new(10, 0, "a", true, null, null),
        new(11, null, "b", null, G(1), Tg("a")),
        new(12, -1, null, false, G(2), null)
    ];

    private static CursorQueryOptions Options(NullOrdering nullOrdering) => new() { NullOrdering = nullOrdering };

    public static TheoryData<string, NullOrdering, bool, int> KeyCases()
    {
        var data = new TheoryData<string, NullOrdering, bool, int>();
        foreach (var key in new[] { "int?", "string", "bool?", "Guid?", "Tag" })
        foreach (var nullOrdering in Enum.GetValues<NullOrdering>())
        foreach (var descending in new[] { false, true })
        foreach (var pageSize in new[] { 1, 3 })
        {
            data.Add(key, nullOrdering, descending, pageSize);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(KeyCases))]
    public void SingleNullableKey_TraversesBothWays_UnderEachConvention(
        string key, NullOrdering nullOrdering, bool descending, int pageSize)
    {
        switch (key)
        {
            case "int?": AssertTraversal(i => i.N, nullOrdering, descending, pageSize); break;
            case "string": AssertTraversal(i => i.S, nullOrdering, descending, pageSize); break;
            case "bool?": AssertTraversal(i => i.B, nullOrdering, descending, pageSize); break;
            case "Guid?": AssertTraversal(i => i.G, nullOrdering, descending, pageSize); break;
            default: AssertTraversal(i => i.T, nullOrdering, descending, pageSize); break;
        }
    }

    [Theory]
    [InlineData(NullOrdering.NullsCompareLower, 1)]
    [InlineData(NullOrdering.NullsCompareLower, 2)]
    [InlineData(NullOrdering.NullsCompareHigher, 1)]
    [InlineData(NullOrdering.NullsCompareHigher, 2)]
    public void MultipleNullableKeys_WithNullTies_TraverseBothWays(NullOrdering nullOrdering, int pageSize)
    {
        var comparerN = NullOrderedSource.Comparer<int?>(nullOrdering);
        var comparerS = NullOrderedSource.Comparer<string?>(nullOrdering);
        var comparerB = NullOrderedSource.Comparer<bool?>(nullOrdering);
        var expected = Items.OrderBy(i => i.N, comparerN).ThenByDescending(i => i.S, comparerS)
            .ThenBy(i => i.B, comparerB).ThenBy(i => i.Id).Select(i => i.Id).ToList();

        var order = NullOrderedSource.Create(Items, nullOrdering).AsCursorQuery(Options(nullOrdering))
            .OrderBy(i => i.N).ThenByDescending(i => i.S).ThenBy(i => i.B).ThenBy(i => i.Id);

        AssertTraversal(order, expected, pageSize);
    }

    [Theory]
    [InlineData(NullOrdering.NullsCompareLower)]
    [InlineData(NullOrdering.NullsCompareHigher)]
    public void StringOrdering_AppliesTheSameConvention_AsTypedOrdering(NullOrdering nullOrdering)
    {
        var expected = Items.OrderByDescending(i => i.N, NullOrderedSource.Comparer<int?>(nullOrdering))
            .ThenBy(i => i.S, NullOrderedSource.Comparer<string?>(nullOrdering)).ThenBy(i => i.Id)
            .Select(i => i.Id).ToList();

        var order = NullOrderedSource.Create(Items, nullOrdering).AsCursorQuery(Options(nullOrdering))
            .OrderBy("N DESC, S, Id");

        AssertTraversal(order, expected, 2);
    }

    [Theory]
    [InlineData(NullOrdering.NullsCompareLower, false)]
    [InlineData(NullOrdering.NullsCompareLower, true)]
    [InlineData(NullOrdering.NullsCompareHigher, false)]
    [InlineData(NullOrdering.NullsCompareHigher, true)]
    public void GuardedOptionalNavigation_TraversesBothWays_AcrossMissingNegativeZeroAndPositiveLeaves(
        NullOrdering nullOrdering, bool descending)
    {
        Node[] nodes =
        [
            new() { Id = 1, Leaf = new Leaf { Value = 0 } },
            new() { Id = 2 },
            new() { Id = 3, Leaf = new Leaf { Value = -4 } },
            new() { Id = 4 },
            new() { Id = 5, Leaf = new Leaf { Value = 7 } },
            new() { Id = 6, Leaf = new Leaf { Value = 0 } },
            new() { Id = 7, Leaf = new Leaf { Value = -4 } }
        ];
        var comparer = NullOrderedSource.Comparer<int?>(nullOrdering);
        int? LeafValue(Node n) => n.Leaf?.Value;
        var expected = (descending
            ? nodes.OrderByDescending(LeafValue, comparer).ThenBy(n => n.Id)
            : nodes.OrderBy(LeafValue, comparer).ThenBy(n => n.Id)).Select(n => n.Id).ToList();

        var order = NullOrderedSource.Create(nodes, nullOrdering).AsCursorQuery(new CursorQueryOptions { NullOrdering = nullOrdering, EnableNullGuards = true })
            .OrderBy(descending ? "Leaf.Value DESC, Id" : "Leaf.Value, Id");

        AssertTraversal(order, expected, 1, n => n.Id);
    }

    [Theory]
    [InlineData(NullOrdering.NullsCompareLower, false, new int[] { 1, 2 })]
    [InlineData(NullOrdering.NullsCompareLower, true, new int[0])]
    [InlineData(NullOrdering.NullsCompareHigher, false, new int[0])]
    [InlineData(NullOrdering.NullsCompareHigher, true, new int[] { 2, 1 })]
    public void NullCursorValue_HasNonNullRowsAfterIt_OnlyWhenNullsSortFirst(
        NullOrdering nullOrdering, bool descending, int[] expectedAfterNull)
    {
        Item[] rows = [new(1, 1, null, null, null, null), new(2, 2, null, null, null, null), new(3, null, null, null, null, null)];
        var source = NullOrderedSource.Create(rows, nullOrdering);
        var order = descending
            ? source.AsCursorQuery(Options(nullOrdering)).OrderByDescending(i => i.N).ThenBy(i => i.Id)
            : source.AsCursorQuery(Options(nullOrdering)).OrderBy(i => i.N).ThenBy(i => i.Id);

        var nullRowCursor = CursorOf(order, 3, i => i.Id);
        var after = order.After(nullRowCursor).ToCursorPage(10);

        Assert.Equal(expectedAfterNull, after.Items.Select(i => i.Id));
    }

    [Theory]
    [InlineData(NullOrdering.NullsCompareLower, false, new int[] { 2 })]
    [InlineData(NullOrdering.NullsCompareLower, true, new int[] { 3 })]
    [InlineData(NullOrdering.NullsCompareHigher, false, new int[] { 2, 3 })]
    [InlineData(NullOrdering.NullsCompareHigher, true, new int[0])]
    public void NonNullCursorValue_IncludesNullRows_OnlyWhenNullsSortAfterIt(
        NullOrdering nullOrdering, bool descending, int[] expectedAfterOne)
    {
        Item[] rows = [new(1, 1, null, null, null, null), new(2, 2, null, null, null, null), new(3, null, null, null, null, null)];
        var source = NullOrderedSource.Create(rows, nullOrdering);
        var order = descending
            ? source.AsCursorQuery(Options(nullOrdering)).OrderByDescending(i => i.N).ThenBy(i => i.Id)
            : source.AsCursorQuery(Options(nullOrdering)).OrderBy(i => i.N).ThenBy(i => i.Id);

        var after = order.After(CursorOf(order, 1, i => i.Id)).ToCursorPage(10);

        Assert.Equal(expectedAfterOne, after.Items.Select(i => i.Id));
    }

    [Fact]
    public void CursorFromOtherConvention_IsRejected_InBothDirections_ForTypedAndStringOrderings()
    {
        var lower = Items.AsQueryable();
        var higher = NullOrderedSource.Create(Items, NullOrdering.NullsCompareHigher);
        var lowerOptions = Options(NullOrdering.NullsCompareLower);
        var higherOptions = Options(NullOrdering.NullsCompareHigher);

        var typedLower = lower.AsCursorQuery(lowerOptions).OrderBy(i => i.N).ThenBy(i => i.Id);
        var typedHigher = higher.AsCursorQuery(higherOptions).OrderBy(i => i.N).ThenBy(i => i.Id);
        var stringLower = lower.AsCursorQuery(lowerOptions).OrderBy("N, Id");
        var stringHigher = higher.AsCursorQuery(higherOptions).OrderBy("N, Id");

        foreach (var (issuer, consumer) in new[]
        {
            (typedLower, typedHigher), (typedHigher, typedLower), (stringLower, stringHigher), (stringHigher, stringLower),
            (typedLower, stringHigher), (stringHigher, typedLower)
        })
        {
            var cursor = issuer.ToCursorPage(2).EndCursor;

            Assert.Throws<InvalidCursorException>(() => consumer.After(cursor).ToCursorPage(2));
            Assert.Throws<InvalidCursorException>(() => consumer.Before(cursor).ToCursorPage(2));
        }

        var sameConvention = typedHigher.ToCursorPage(2).EndCursor;
        Assert.Equal(
            typedHigher.After(sameConvention).ToCursorPage(2).Items.Select(i => i.Id),
            stringHigher.After(sameConvention).ToCursorPage(2).Items.Select(i => i.Id));
    }

    [Fact]
    public void AsCursorQuery_WithoutOptions_UsesTheDefaultConvention()
    {
        Assert.Equal(NullOrdering.NullsCompareLower, StateOf(Items.AsQueryable().AsCursorQuery()).NullOrdering);
        Assert.Equal(NullOrdering.NullsCompareLower, StateOf(Items.AsQueryable().AsCursorQuery(null)).NullOrdering);
        Assert.Equal(
            NullOrdering.NullsCompareLower,
            StateOf(Items.AsQueryable().AsCursorQuery(new CursorQueryOptions())).NullOrdering);
        Assert.Equal(
            NullOrdering.NullsCompareLower,
            StateOf(Items.AsQueryable().AsCursorQuery(Options(NullOrdering.NullsCompareLower))).NullOrdering);
    }

    [Fact]
    public void AsCursorQuery_WithExplicitOptions_UsesThatConvention()
    {
        var query = Items.AsQueryable().AsCursorQuery(Options(NullOrdering.NullsCompareHigher));

        Assert.Equal(NullOrdering.NullsCompareHigher, StateOf(query).NullOrdering);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    [InlineData(99)]
    public void CursorQueryOptions_UndefinedNullOrdering_IsRejected(int value)
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(
            () => new CursorQueryOptions { NullOrdering = (NullOrdering)value });

        Assert.Equal("value", error.ParamName);
    }

    [Fact]
    public void AsCursorQuery_NullSource_IsRejected()
    {
        Assert.Throws<ArgumentNullException>(() => ((IQueryable<Item>)null!).AsCursorQuery());
    }

    [Fact]
    public void AsCursorQuery_CopiesTheOptions_SoLaterChangesDoNotAffectTheQuery()
    {
        var options = Options(NullOrdering.NullsCompareHigher);
        var query = Items.AsQueryable().AsCursorQuery(options).OrderBy(i => i.Id);

        options.NullOrdering = NullOrdering.NullsCompareLower;

        Assert.Equal(NullOrdering.NullsCompareHigher, StateOf(query).NullOrdering);
    }

    [Fact]
    public void Convention_IsPreservedThroughOrderingAndNavigation()
    {
        const NullOrdering expected = NullOrdering.NullsCompareHigher;
        var source = Items.AsQueryable();
        var start = source.AsCursorQuery(Options(expected));

        var ordered = start.OrderBy(i => i.N);
        var cursor = NullOrderedSource.Create(Items, expected).AsCursorQuery(Options(expected))
            .OrderBy(i => i.N).ThenBy(i => i.Id).ToCursorPage(2).EndCursor;

        IExecutableCursorQuery<Item>[] steps =
        [
            ordered,
            start.OrderByDescending(i => i.N),
            start.OrderBy("N DESC"),
            ordered.ThenBy(i => i.Id),
            ordered.ThenByDescending(i => i.Id),
            ordered.ThenBy("Id"),
            ((ICursorQuery<Item>)ordered).OrderBy(i => i.Id),
            ordered.ThenBy(i => i.Id).After(cursor),
            ordered.ThenBy(i => i.Id).Before(cursor),
            ordered.After(null),
            ordered.After(cursor)
        ];

        Assert.All(steps, step => Assert.Equal(expected, StateOf(step).NullOrdering));
    }

    private static CursorQueryState<Item> StateOf(object query)
    {
        return ((CursorQuery<Item>)query).State;
    }

    private static void AssertTraversal<TKey>(
        Expression<Func<Item, TKey>> key, NullOrdering nullOrdering, bool descending, int pageSize)
    {
        var comparer = NullOrderedSource.Comparer<TKey>(nullOrdering);
        var compiled = key.Compile();
        var expected = (descending
            ? Items.OrderByDescending(compiled, comparer).ThenBy(i => i.Id)
            : Items.OrderBy(compiled, comparer).ThenBy(i => i.Id)).Select(i => i.Id).ToList();

        var source = NullOrderedSource.Create(Items, nullOrdering).AsCursorQuery(Options(nullOrdering));
        var order = descending
            ? source.OrderByDescending(key).ThenBy(i => i.Id)
            : source.OrderBy(key).ThenBy(i => i.Id);

        AssertTraversal(order, expected, pageSize);
    }

    private static void AssertTraversal(IOrderedCursorQuery<Item> order, List<int> expected, int pageSize)
    {
        AssertTraversal(order, expected, pageSize, i => i.Id);
    }

    private static void AssertTraversal<T>(
        IOrderedCursorQuery<T> order, List<int> expected, int pageSize, Func<T, int> id)
    {
        var forward = new List<int>();
        var page = order.ToCursorPage(pageSize);
        var firstCursor = page.StartCursor;
        while (true)
        {
            forward.AddRange(page.Items.Select(id));
            Assert.True(forward.Count <= expected.Count, "Forward traversal did not terminate.");
            if (!page.HasNextPage) break;
            page = order.After(page.EndCursor).ToCursorPage(pageSize);
        }

        Assert.Equal(expected, forward);
        Assert.Empty(order.After(page.EndCursor).ToCursorPage(pageSize).Items);

        var backward = new List<int>();
        while (true)
        {
            backward.InsertRange(0, page.Items.Select(id));
            Assert.True(backward.Count <= expected.Count, "Backward traversal did not terminate.");
            if (!page.HasPreviousPage) break;
            page = order.Before(page.StartCursor).ToCursorPage(pageSize);
        }

        Assert.Equal(expected, backward);
        Assert.Empty(order.Before(firstCursor).ToCursorPage(pageSize).Items);
    }

    private static string CursorOf<T>(IOrderedCursorQuery<T> order, int id, Func<T, int> idOf)
    {
        var page = order.ToCursorPage(1);
        while (true)
        {
            if (idOf(page.Items[0]) == id)
            {
                return page.EndCursor!;
            }

            Assert.True(page.HasNextPage, $"Row {id} was not found.");
            page = order.After(page.EndCursor).ToCursorPage(1);
        }
    }
}
