using Zift.Pagination.Cursor;

namespace Zift.Pagination.Tests;

public class MaxOrderingKeysTests
{
    private sealed record Row(int Id, string Name);

    private static IQueryable<Row> Source { get; } = Array.Empty<Row>().AsQueryable();

    private static string Keys(int count) => string.Join(",", Enumerable.Repeat("Id", count));

    private static ICursorQuery<Row> CursorQuery(int maxOrderingKeys) =>
        Source.AsCursorQuery(new CursorQueryOptions { MaxOrderingKeys = maxOrderingKeys });

    [Fact]
    public void MaxOrderingKeys_DefaultsToTheDocumentedConstant()
    {
        Assert.Equal(16, CursorQueryOptions.DefaultMaxOrderingKeys);
        Assert.Equal(CursorQueryOptions.DefaultMaxOrderingKeys, new CursorQueryOptions().MaxOrderingKeys);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MaxOrderingKeys_RejectsValuesBelowOne(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CursorQueryOptions { MaxOrderingKeys = value });
    }

    [Fact]
    public void OrderBy_String_AcceptsExactlyTheMaximum()
    {
        var query = CursorQuery(3).OrderBy(Keys(3));

        Assert.NotNull(query);
    }

    [Fact]
    public void OrderBy_String_ThrowsBeyondTheMaximum()
    {
        var error = Assert.Throws<InvalidOrderingClauseException>(() => CursorQuery(3).OrderBy(Keys(4)));

        Assert.Equal("The ordering exceeds the maximum of 3 keys.", error.Message);
    }

    [Fact]
    public void OrderBy_String_UsesTheDefaultMaximumWhenNoOptionsAreGiven()
    {
        var max = CursorQueryOptions.DefaultMaxOrderingKeys;

        Assert.NotNull(Source.AsCursorQuery().OrderBy(Keys(max)));
        Assert.Throws<InvalidOrderingClauseException>(() => Source.AsCursorQuery().OrderBy(Keys(max + 1)));
    }

    [Fact]
    public void OrderBy_String_RejectsTooManyEntriesBeforeResolvingAnyProperty()
    {
        var error = Assert.Throws<InvalidOrderingClauseException>(() =>
            CursorQuery(3).OrderBy($"{Keys(3)},Nope"));

        Assert.Equal("The ordering exceeds the maximum of 3 keys.", error.Message);
    }

    [Fact]
    public void ThenBy_String_RejectsTooManyEntriesGivenTheRemainingCapacity()
    {
        var query = CursorQuery(3).OrderBy(r => r.Name);

        var error = Assert.Throws<InvalidOrderingClauseException>(() => query.ThenBy("Id,Id,Nope"));

        Assert.Equal("The ordering exceeds the maximum of 3 keys.", error.Message);
    }

    [Fact]
    public void ThenBy_String_OnAFullOrdering_NamesTheMaximum()
    {
        var query = CursorQuery(2).OrderBy(Keys(2));

        var error = Assert.Throws<InvalidOrderingClauseException>(() => query.ThenBy("Id"));

        Assert.Equal("The ordering exceeds the maximum of 2 keys.", error.Message);
    }

    [Theory]
    [InlineData("Id,Id,Id,", 3)]
    [InlineData("Id,,Id", 3)]
    [InlineData("Id, ,Id", 3)]
    public void OrderBy_String_ReportsAnEmptyEntryNotTheLimit(string clause, int max)
    {
        var error = Assert.Throws<InvalidOrderingClauseException>(() => CursorQuery(max).OrderBy(clause));

        Assert.Equal("The order-by clause contains an empty entry.", error.Message);
    }

    [Fact]
    public void ThenBy_CountsTowardTheSameMaximum()
    {
        var query = CursorQuery(3).OrderBy(r => r.Name).ThenBy(r => r.Id);

        Assert.NotNull(query.ThenBy("Id"));
        Assert.Throws<InvalidOrderingClauseException>(() => query.ThenBy("Id").ThenBy(r => r.Id));
    }

    [Fact]
    public void ThenBy_String_ThrowsWhenTheTotalExceedsTheMaximum()
    {
        var query = CursorQuery(3).OrderBy(r => r.Name);

        Assert.NotNull(query.ThenBy(Keys(2)));
        Assert.Throws<InvalidOrderingClauseException>(() => query.ThenBy(Keys(3)));
    }

    [Fact]
    public void ThenByDescending_Selector_ThrowsBeyondTheMaximum()
    {
        var query = CursorQuery(2).OrderByDescending(r => r.Name).ThenByDescending(r => r.Id);

        Assert.Throws<InvalidOrderingClauseException>(() => query.ThenByDescending(r => r.Id));
    }
}
