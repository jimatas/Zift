using Zift.Pagination.Cursor;

namespace Zift.Pagination.Hardening;

public class CursorRegressionTests
{
    private sealed record TextRow(int Id, string Key);
    private sealed record ValueRow(int Id, ComparableValue Key);
    private sealed record ReferenceRow(int Id, ComparableReference? Key);
    private sealed record BooleanRow(int Id, bool? Key);
    private sealed record Parent(int Id, Child? Child);
    private sealed record Child(int Rank);
    private sealed record Ancestor(int Id, Branch? Branch);
    private sealed record Branch(Child Child);

    public readonly struct ComparableValue(int value) : IComparable<ComparableValue>
    {
        public int Value { get; init; } = value;
        public int CompareTo(ComparableValue other) => Value.CompareTo(other.Value);
    }

    public sealed class ComparableReference(int value) : IComparable<ComparableReference>
    {
        public int Value { get; } = value;
        public int CompareTo(ComparableReference? other) => other is null ? 1 : Value.CompareTo(other.Value);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CultureEquivalentStrings_TraverseBothWays_WithUniqueTieBreaker(bool descending)
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Assert.Equal(0, string.Compare("a", "a\u00ad"));
            Assert.NotEqual("a", "a\u00ad");
            var rows = new[] { new TextRow(1, "a"), new TextRow(2, "a\u00ad"), new TextRow(3, "b") }.AsQueryable();
            var order = descending ? rows.AsCursorQuery().OrderByDescending(r => r.Key) : rows.AsCursorQuery().OrderBy(r => r.Key);
            AssertTraversal(order.ThenBy(r => r.Id), r => r.Id, descending ? [3, 1, 2] : [1, 2, 3]);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ComparableValueKeys_TraverseBothWays_WithUniqueTieBreaker(bool descending)
    {
        var rows = new[] { new ValueRow(1, new(4)), new ValueRow(2, new(4)), new ValueRow(3, new(5)) }.AsQueryable();
        var order = descending ? rows.AsCursorQuery().OrderByDescending(r => r.Key) : rows.AsCursorQuery().OrderBy(r => r.Key);
        AssertTraversal(order.ThenBy(r => r.Id), r => r.Id, descending ? [3, 1, 2] : [1, 2, 3]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ComparableReferenceKeys_TraverseBothWays_WithUniqueTieBreaker(bool descending)
    {
        var rows = new[] { new ReferenceRow(1, null), new ReferenceRow(2, new(4)), new ReferenceRow(3, new(4)), new ReferenceRow(4, new(5)) }.AsQueryable();
        var order = descending ? rows.AsCursorQuery().OrderByDescending(r => r.Key) : rows.AsCursorQuery().OrderBy(r => r.Key);
        AssertTraversal(order.ThenBy(r => r.Id), r => r.Id, descending ? [4, 2, 3, 1] : [1, 2, 3, 4]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GuardedValuePaths_PreserveMissingNegativeAndZeroKeys(bool descending)
    {
        var rows = new[] { new Parent(1, null), new Parent(2, new(-1)), new Parent(3, new(0)), new Parent(4, null), new Parent(5, new(1)) }.AsQueryable();
        var order = rows.AsCursorQuery().OrderBy(descending ? "Child.Rank DESC, Id" : "Child.Rank, Id",
            new OrderingOptions { EnableNullGuards = true });
        AssertTraversal(order, r => r.Id, descending ? [5, 3, 2, 1, 4] : [1, 4, 2, 3, 5]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GuardedMultiLevelPaths_ShortCircuitMissingNavigations_AndLiftValueLeaves(bool descending)
    {
        var rows = new[]
        {
            new Ancestor(1, null), new Ancestor(2, new(null!)),
            new Ancestor(3, new(new(-1))), new Ancestor(4, new(new(0))),
            new Ancestor(5, new(new(1))), new Ancestor(6, null)
        }.AsQueryable();
        var order = rows.AsCursorQuery().OrderBy(
            descending ? "Branch.Child.Rank DESC, Id" : "Branch.Child.Rank, Id",
            new OrderingOptions { EnableNullGuards = true });

        AssertTraversal(order, r => r.Id, descending ? [5, 4, 3, 1, 2, 6] : [1, 2, 6, 3, 4, 5]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NullableBooleanKeys_TraverseBothWays_WithoutEvaluatingMissingValues(bool descending)
    {
        var rows = new[]
        {
            new BooleanRow(1, null), new BooleanRow(2, false), new BooleanRow(3, true),
            new BooleanRow(4, null), new BooleanRow(5, false), new BooleanRow(6, true)
        }.AsQueryable();
        var order = descending ? rows.AsCursorQuery().OrderByDescending(r => r.Key) : rows.AsCursorQuery().OrderBy(r => r.Key);
        AssertTraversal(order.ThenBy(r => r.Id), r => r.Id, descending ? [3, 6, 2, 5, 1, 4] : [1, 4, 2, 5, 3, 6]);
    }

    private static void AssertTraversal<T>(IOrderedCursorQuery<T> order, Func<T, int> id, int[] expected)
    {
        var forward = new List<int>();
        var page = order.ToCursorPage(1);
        var firstCursor = page.StartCursor;
        while (true)
        {
            forward.AddRange(page.Items.Select(id));
            Assert.True(forward.Count <= expected.Length, "Traversal did not terminate.");
            if (!page.HasNextPage) break;
            page = order.After(page.EndCursor).ToCursorPage(1);
        }
        Assert.Equal(expected, forward);
        Assert.Empty(order.After(page.EndCursor).ToCursorPage(1).Items);

        var backward = new List<int>();
        while (true)
        {
            backward.InsertRange(0, page.Items.Select(id));
            Assert.True(backward.Count <= expected.Length, "Traversal did not terminate.");
            if (!page.HasPreviousPage) break;
            page = order.Before(page.StartCursor).ToCursorPage(1);
        }
        Assert.Equal(expected, backward);
        Assert.Empty(order.Before(firstCursor).ToCursorPage(1).Items);
    }
}
