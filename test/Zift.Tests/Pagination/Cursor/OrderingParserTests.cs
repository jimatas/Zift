namespace Zift.Pagination.Cursor;

using Fixture;
using Ordering;

public sealed class OrderingParserTests
{
    private readonly OrderingOptions _defaultOptions = new();

    [Fact]
    public void Parse_EmptyClause_ThrowsFormatException()
    {
        var ex = Assert.Throws<FormatException>(() =>
            Ordering<TestClass>.Parse("Int32Value,", _defaultOptions));

        Assert.Contains("contains an empty clause", ex.Message);
    }

    [Fact]
    public void Parse_InvalidDirection_ThrowsFormatException()
    {
        var ex = Assert.Throws<FormatException>(() =>
            Ordering<TestClass>.Parse("Int32Value down", _defaultOptions));

        Assert.Contains("Must be 'ASC' or 'DESC'", ex.Message);
    }

    [Fact]
    public void Parse_TooManyParts_ThrowsFormatException()
    {
        var ex = Assert.Throws<FormatException>(() =>
            Ordering<TestClass>.Parse("Int32Value asc extra", _defaultOptions));

        Assert.Contains("Expected format is 'Property [ASC|DESC]'", ex.Message);
    }

    [Fact]
    public void Parse_NestedPropertyPathWithDirection_AddsDescendingClause()
    {
        var ordering = Ordering<TestClass>.Parse("NestedEntity.Int32Value DESC", _defaultOptions);

        var clause = Assert.Single(ordering.Clauses);
        Assert.Equal(OrderingDirection.Descending, clause.Direction);
    }

    [Fact]
    public void Parse_InvalidPropertySegment_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Ordering<TestClass>.Parse("NestedEntity.DoesNotExist ASC", _defaultOptions));

        Assert.Contains("is not defined for type", ex.Message);
    }
}
