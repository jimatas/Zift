using Zift.Pagination.Cursor;

namespace Zift.Pagination.Hardening;

public class CursorPaginationTests
{
    private enum Status { Draft, Active, Archived }

    private sealed record Row(
        int Id,
        string Name,
        string? Nick = null,
        int? Score = null,
        Status Status = Status.Draft,
        bool Flag = false,
        Guid Key = default,
        DateTime Created = default,
        decimal Price = 0m);

    private static IQueryable<Row> Rows(params Row[] rows)
    {
        return rows.AsQueryable();
    }

    private static IQueryable<Row> Numbered(int count)
    {
        return Enumerable.Range(1, count).Select(i => new Row(i, $"N{i:00}")).AsQueryable();
    }

    private static List<int> WalkForward(
        IQueryable<Row> source,
        Func<ICursorQuery<Row>, IOrderedCursorQuery<Row>> order,
        int pageSize)
    {
        var ids = new List<int>();
        string? cursor = null;
        var guard = 0;

        do
        {
            var page = order(source.AsCursorQuery()).After(cursor).ToCursorPage(pageSize);
            ids.AddRange(page.Items.Select(r => r.Id));
            cursor = page.HasNextPage ? page.EndCursor : null;
            Assert.True(++guard < 1000, "pagination did not terminate");
        }
        while (cursor is not null);

        return ids;
    }

    [Fact]
    public void ForwardTraversal_VisitsEveryRowOnce_InOrder()
    {
        var ids = WalkForward(Numbered(10), q => q.OrderBy(r => r.Id), pageSize: 3);

        Assert.Equal(Enumerable.Range(1, 10), ids);
    }

    [Fact]
    public void ForwardTraversal_Descending_VisitsEveryRowOnce_InOrder()
    {
        var ids = WalkForward(Numbered(10), q => q.OrderByDescending(r => r.Id), pageSize: 4);

        Assert.Equal(Enumerable.Range(1, 10).Reverse(), ids);
    }

    [Fact]
    public void FirstPage_HasNextButNoPrevious()
    {
        var page = Numbered(10).AsCursorQuery().OrderBy(r => r.Id).ToCursorPage(3);

        Assert.Equal([1, 2, 3], page.Items.Select(r => r.Id));
        Assert.True(page.HasNextPage);
        Assert.False(page.HasPreviousPage);
    }

    [Fact]
    public void LastPage_HasPreviousButNoNext()
    {
        var source = Numbered(10);
        var first = source.AsCursorQuery().OrderBy(r => r.Id).ToCursorPage(8);
        var last = source.AsCursorQuery().OrderBy(r => r.Id).After(first.EndCursor).ToCursorPage(8);

        Assert.Equal([9, 10], last.Items.Select(r => r.Id));
        Assert.False(last.HasNextPage);
        Assert.True(last.HasPreviousPage);
    }

    [Fact]
    public void ExactlyOnePageOfData_HasNoNextPage()
    {
        var page = Numbered(3).AsCursorQuery().OrderBy(r => r.Id).ToCursorPage(3);

        Assert.Equal(3, page.Items.Count);
        Assert.False(page.HasNextPage);
    }

    [Fact]
    public void EmptySource_ReturnsEmptyPage_WithoutCursors()
    {
        var page = Rows().AsCursorQuery().OrderBy(r => r.Id).ToCursorPage(5);

        Assert.Empty(page.Items);
        Assert.Null(page.StartCursor);
        Assert.Null(page.EndCursor);
        Assert.False(page.HasNextPage);
        Assert.False(page.HasPreviousPage);
    }

    [Fact]
    public void BackwardTraversal_ReturnsItemsInOriginalOrder()
    {
        var source = Numbered(10);
        Func<ICursorQuery<Row>, IOrderedCursorQuery<Row>> order = q => q.OrderBy(r => r.Id);

        var page2 = order(source.AsCursorQuery()).After(order(source.AsCursorQuery()).ToCursorPage(3).EndCursor).ToCursorPage(3);
        Assert.Equal([4, 5, 6], page2.Items.Select(r => r.Id));

        var back = order(source.AsCursorQuery()).Before(page2.StartCursor).ToCursorPage(3);

        Assert.Equal([1, 2, 3], back.Items.Select(r => r.Id));
        Assert.True(back.HasNextPage);
        Assert.False(back.HasPreviousPage);
    }

    [Fact]
    public void BackwardTraversal_WithMoreRowsBefore_HasPreviousPage()
    {
        var source = Numbered(10);
        Func<ICursorQuery<Row>, IOrderedCursorQuery<Row>> order = q => q.OrderBy(r => r.Id);
        var cursorAt7 = order(source.AsCursorQuery()).After(
            order(source.AsCursorQuery()).ToCursorPage(6).EndCursor).ToCursorPage(1).StartCursor;

        var back = order(source.AsCursorQuery()).Before(cursorAt7).ToCursorPage(3);

        Assert.Equal([4, 5, 6], back.Items.Select(r => r.Id));
        Assert.True(back.HasPreviousPage);
        Assert.True(back.HasNextPage);
    }

    [Fact]
    public void NonUniqueFirstKey_WithUniqueTieBreaker_SkipsNothing()
    {
        var source = Rows(
            new Row(1, "A"), new Row(2, "A"), new Row(3, "A"), new Row(4, "B"), new Row(5, "B"));

        var ids = WalkForward(source, q => q.OrderBy(r => r.Name).ThenBy(r => r.Id), pageSize: 1);

        Assert.Equal([1, 2, 3, 4, 5], ids);
    }

    [Fact]
    public void MixedDirections_AcrossKeys_TraverseCorrectly()
    {
        var source = Rows(
            new Row(1, "A"), new Row(2, "A"), new Row(3, "B"), new Row(4, "B"), new Row(5, "C"));

        var ids = WalkForward(source, q => q.OrderBy(r => r.Name).ThenByDescending(r => r.Id), pageSize: 2);

        Assert.Equal([2, 1, 4, 3, 5], ids);
    }

    [Fact]
    public void NullableValueKey_NullsSortFirst_WhenAscending()
    {
        var source = Rows(
            new Row(1, "a", Score: 5), new Row(2, "b", Score: null), new Row(3, "c", Score: 1), new Row(4, "d", Score: null));

        var ids = WalkForward(source, q => q.OrderBy(r => r.Score).ThenBy(r => r.Id), pageSize: 1);

        Assert.Equal([2, 4, 3, 1], ids);
    }

    [Fact]
    public void NullableValueKey_NullsSortLast_WhenDescending()
    {
        var source = Rows(
            new Row(1, "a", Score: 5), new Row(2, "b", Score: null), new Row(3, "c", Score: 1), new Row(4, "d", Score: null));

        var ids = WalkForward(source, q => q.OrderByDescending(r => r.Score).ThenBy(r => r.Id), pageSize: 1);

        Assert.Equal([1, 3, 2, 4], ids);
    }

    [Fact]
    public void NullableReferenceKey_NullsSortFirst_WhenAscending()
    {
        var source = Rows(
            new Row(1, "a", Nick: "z"), new Row(2, "b", Nick: null), new Row(3, "c", Nick: "m"), new Row(4, "d", Nick: null));

        var ids = WalkForward(source, q => q.OrderBy(r => r.Nick).ThenBy(r => r.Id), pageSize: 1);

        Assert.Equal([2, 4, 3, 1], ids);
    }

    [Fact]
    public void NullableReferenceKey_NullsSortLast_WhenDescending()
    {
        var source = Rows(
            new Row(1, "a", Nick: "z"), new Row(2, "b", Nick: null), new Row(3, "c", Nick: "m"), new Row(4, "d", Nick: null));

        var ids = WalkForward(source, q => q.OrderByDescending(r => r.Nick).ThenBy(r => r.Id), pageSize: 1);

        Assert.Equal([1, 3, 2, 4], ids);
    }

    [Fact]
    public void EnumKey_Traverses()
    {
        var source = Rows(
            new Row(1, "a", Status: Status.Archived), new Row(2, "b", Status: Status.Draft), new Row(3, "c", Status: Status.Active));

        Assert.Equal([2, 3, 1], WalkForward(source, q => q.OrderBy(r => r.Status).ThenBy(r => r.Id), 1));
        Assert.Equal([1, 3, 2], WalkForward(source, q => q.OrderByDescending(r => r.Status).ThenBy(r => r.Id), 1));
    }

    [Fact]
    public void BoolKey_Traverses()
    {
        var source = Rows(
            new Row(1, "a", Flag: true), new Row(2, "b", Flag: false), new Row(3, "c", Flag: true), new Row(4, "d", Flag: false));

        Assert.Equal([2, 4, 1, 3], WalkForward(source, q => q.OrderBy(r => r.Flag).ThenBy(r => r.Id), 1));
        Assert.Equal([1, 3, 2, 4], WalkForward(source, q => q.OrderByDescending(r => r.Flag).ThenBy(r => r.Id), 1));
    }

    [Fact]
    public void GuidKey_Traverses()
    {
        var source = Rows(
            new Row(1, "a", Key: Guid.Parse("00000000-0000-0000-0000-000000000003")),
            new Row(2, "b", Key: Guid.Parse("00000000-0000-0000-0000-000000000001")),
            new Row(3, "c", Key: Guid.Parse("00000000-0000-0000-0000-000000000002")));

        Assert.Equal([2, 3, 1], WalkForward(source, q => q.OrderBy(r => r.Key), 1));
    }

    [Fact]
    public void DateTimeAndDecimalKeys_Traverse()
    {
        var source = Rows(
            new Row(1, "a", Created: new DateTime(2024, 3, 1), Price: 9.99m),
            new Row(2, "b", Created: new DateTime(2024, 1, 1), Price: 19.99m),
            new Row(3, "c", Created: new DateTime(2024, 2, 1), Price: 1.50m));

        Assert.Equal([2, 3, 1], WalkForward(source, q => q.OrderBy(r => r.Created), 1));
        Assert.Equal([3, 1, 2], WalkForward(source, q => q.OrderBy(r => r.Price), 1));
    }

    [Fact]
    public void StringOrdering_MatchesTypedOrdering()
    {
        var source = Rows(
            new Row(1, "B"), new Row(2, "A"), new Row(3, "B"), new Row(4, "A"));

        var typed = WalkForward(source, q => q.OrderByDescending(r => r.Name).ThenBy(r => r.Id), 3);
        var parsed = WalkForward(source, q => q.OrderBy("name desc, ID"), 3);

        Assert.Equal([1, 3, 2, 4], typed);
        Assert.Equal(typed, parsed);
    }

    [Fact]
    public void Cursor_IsUrlSafe()
    {
        var page = Numbered(5).AsCursorQuery().OrderBy(r => r.Id).ToCursorPage(2);

        Assert.DoesNotContain('+', page.EndCursor!);
        Assert.DoesNotContain('/', page.EndCursor!);
        Assert.DoesNotContain('=', page.EndCursor!);
    }

    [Fact]
    public void Cursor_IsAccepted_WhenLambdaParameterNamesDiffer()
    {
        var source = Numbered(5);
        var first = source.AsCursorQuery().OrderBy(a => a.Id).ToCursorPage(2);

        var second = source.AsCursorQuery().OrderBy(zzz => zzz.Id).After(first.EndCursor).ToCursorPage(2);

        Assert.Equal([3, 4], second.Items.Select(r => r.Id));
    }

    [Fact]
    public void Cursor_FromDifferentOrdering_IsRejected()
    {
        var source = Numbered(5);
        var byId = source.AsCursorQuery().OrderBy(r => r.Id).ToCursorPage(2);

        Assert.Throws<InvalidCursorException>(() =>
            source.AsCursorQuery().OrderByDescending(r => r.Id).After(byId.EndCursor).ToCursorPage(2));
        Assert.Throws<InvalidCursorException>(() =>
            source.AsCursorQuery().OrderBy(r => r.Name).After(byId.EndCursor).ToCursorPage(2));
    }

    [Theory]
    [InlineData("not a cursor")]
    [InlineData("e30")]
    [InlineData("W10")]
    [InlineData("AAAA")]
    public void MalformedCursor_ThrowsInvalidCursorException(string cursor)
    {
        Assert.Throws<InvalidCursorException>(() =>
            Numbered(5).AsCursorQuery().OrderBy(r => r.Id).After(cursor).ToCursorPage(2));
    }

    [Fact]
    public void ThenByString_AppendsParsedClauses()
    {
        var source = Rows(new Row(1, "A"), new Row(2, "A"), new Row(3, "B"));

        var ids = WalkForward(source, q => q.OrderBy(r => r.Name).ThenBy("id desc"), 1);

        Assert.Equal([2, 1, 3], ids);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void InvalidPageSize_Throws(int pageSize)
    {
        var query = Numbered(3).AsCursorQuery().OrderBy(r => r.Id);

        Assert.Throws<ArgumentOutOfRangeException>(() => query.ToCursorPage(pageSize));
    }

    [Fact]
    public void LargePageSize_BelowTheMaximum_Works()
    {
        var page = Numbered(3).AsCursorQuery().OrderBy(r => r.Id).ToCursorPage(int.MaxValue - 1);

        Assert.Equal(3, page.Items.Count);
        Assert.False(page.HasNextPage);
    }

    [Fact]
    public void KeySelectorReturningObject_IsRejected()
    {
        var query = Numbered(3).AsCursorQuery();

        Assert.Throws<ArgumentException>(() => query.OrderBy<object>(r => r.Id));
    }

    [Fact]
    public void NestedKey_WithNullGuards_HandlesNullNavigation()
    {
        var source = new[]
        {
            new Parent(1, new Child("b")),
            new Parent(2, null),
            new Parent(3, new Child("a"))
        }.AsQueryable();

        var options = new OrderingOptions { EnableNullGuards = true };
        var page = source.AsCursorQuery().OrderBy("child.name, id", options).ToCursorPage(10);

        Assert.Equal([2, 3, 1], page.Items.Select(p => p.Id));
    }

    private sealed record Child(string Name);

    private sealed record Parent(int Id, Child? Child);
}
