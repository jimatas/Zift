using Zift.Pagination.Cursor;

namespace Zift.Pagination.Hardening;

public class InvalidOrderingClauseExceptionTests
{
    private sealed record Row(int Id, string Name);

    private static IQueryable<Row> Source { get; } = Array.Empty<Row>().AsQueryable();

    [Fact]
    public void InvalidOrderingClauseException_IsAFormatException()
    {
        Assert.True(typeof(FormatException).IsAssignableFrom(typeof(InvalidOrderingClauseException)));
    }

    [Theory]
    [InlineData("Id,,Name", "The order-by clause contains an empty entry.")]
    [InlineData("Id ASC extra", "Invalid order-by entry 'Id ASC extra'. Expected 'Property [ASC|DESC]'.")]
    [InlineData("Id Sideways", "Invalid order-by direction 'Sideways'. Must be 'ASC' or 'DESC'.")]
    [InlineData("Nope", "'Row' has no public property 'Nope'.")]
    public void OrderBy_ThrowsForEachKindOfInvalidEntry(string clause, string message)
    {
        var error = Assert.Throws<InvalidOrderingClauseException>(() => Source.AsCursorQuery().OrderBy(clause));

        Assert.Equal(message, error.Message);
    }

    [Fact]
    public void ThenBy_ThrowsForInvalidEntry()
    {
        var query = Source.AsCursorQuery().OrderBy(r => r.Id);

        Assert.Throws<InvalidOrderingClauseException>(() => query.ThenBy("Nope"));
    }

    private sealed class OddMembers
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public int value { get; set; }
        public int WriteOnly { set { } }
        public int this[int index] => index;
    }

    [Theory]
    [InlineData("WriteOnly", "Property 'WriteOnly' on type 'OddMembers' can't be read.")]
    [InlineData("Item", "Indexed property 'Item' on type 'OddMembers' can't be used.")]
    [InlineData("Value", "Property 'Value' is ambiguous on type 'OddMembers'.")]
    public void OrderBy_ThrowsForUnusableProperties(string clause, string message)
    {
        var source = Array.Empty<OddMembers>().AsQueryable();

        var error = Assert.Throws<InvalidOrderingClauseException>(() => source.AsCursorQuery().OrderBy(clause));

        Assert.Equal(message, error.Message);
    }

    [Fact]
    public void OrderBy_KeepsTheOriginalExceptionForAmbiguousProperties()
    {
        var source = Array.Empty<OddMembers>().AsQueryable();

        var error = Assert.Throws<InvalidOrderingClauseException>(() => source.AsCursorQuery().OrderBy("value"));

        Assert.IsType<System.Reflection.AmbiguousMatchException>(error.InnerException);
    }

    [Fact]
    public void InvalidCursor_IsNotAnInvalidOrderingClause()
    {
        var query = Source.AsCursorQuery().OrderBy("Id");

        Assert.Throws<InvalidCursorException>(() => query.After("garbage").ToCursorPage(2));
    }
}
