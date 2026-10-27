using Zift.Pagination.Cursor;

namespace Zift.Pagination.Hardening;

public class OrderingParserTests
{
    private sealed record Item(int Id, string Name);
    private sealed record Parent(int Id, Item? Child);

    private static IQueryable<Item> Items => new[] { new Item(1, "A") }.AsQueryable();

    [Theory]
    [InlineData("Missing")]
    [InlineData("Id Sideways")]
    [InlineData("Id ASC extra")]
    [InlineData("Id,,Name")]
    [InlineData("Id.Name")]
    public void OrderBy_InvalidClause_ThrowsInvalidOrderingClauseException(string clause)
    {
        Assert.Throws<InvalidOrderingClauseException>(() => Items.AsCursorQuery().OrderBy(clause));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void OrderBy_BlankClause_ThrowsArgumentException(string clause)
    {
        Assert.ThrowsAny<ArgumentException>(() => Items.AsCursorQuery().OrderBy(clause));
    }

    [Fact]
    public void OrderBy_AcceptsMixedCaseAndWhitespace()
    {
        var page = Items.AsCursorQuery().OrderBy("  nAmE   DeSc ,  id  ").ToCursorPage(5);

        Assert.Single(page.Items);
    }

    [Fact]
    public void OrderBy_NestedMixedCasePath_ProducesInterchangeableCursors()
    {
        var rows = new[]
        {
            new Parent(1, null), new Parent(2, new Item(0, "A")), new Parent(3, new Item(1, "B"))
        }.AsQueryable();
        var options = new OrderingOptions { EnableNullGuards = true };
        var canonical = rows.AsCursorQuery().OrderBy("Child.Id, Id", options);
        var mixedCase = rows.AsCursorQuery().OrderBy("cHiLd.iD aSc, iD", options);
        var first = canonical.ToCursorPage(1);

        Assert.Equal(first.EndCursor, mixedCase.ToCursorPage(1).EndCursor);
        var next = mixedCase.After(first.EndCursor).ToCursorPage(1);
        Assert.Equal(2, Assert.Single(next.Items).Id);
        Assert.Equal(canonical.After(first.EndCursor).ToCursorPage(1).EndCursor, next.EndCursor);
    }

    [Theory]
    [InlineData(".Child.Id", "'Parent' has no public property ''.")]
    [InlineData("Child..Id", "'Item' has no public property ''.")]
    [InlineData("Child.Id.", "'Int32' has no public property ''.")]
    [InlineData("Child.Missing", "'Item' has no public property 'Missing'.")]
    public void OrderBy_MalformedNestedPath_PreservesExceptionMessage(string path, string expectedMessage)
    {
        var rows = Array.Empty<Parent>().AsQueryable();

        var exception = Assert.Throws<InvalidOrderingClauseException>(() => rows.AsCursorQuery().OrderBy(path));

        Assert.Equal(expectedMessage, exception.Message);
    }
}
