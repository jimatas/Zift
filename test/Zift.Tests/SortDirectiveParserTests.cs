namespace Zift.Tests;

using SharedFixture.Models;
using Sorting;
using Sorting.Dynamic;

public class SortDirectiveParserTests
{
    private readonly SortDirectiveParser<Product> _parser = new();

    [Theory]
    [InlineData("Name", "Name", SortDirection.Ascending)]
    [InlineData("Price DESC", "Price", SortDirection.Descending)]
    [InlineData("Name ASC", "Name", SortDirection.Ascending)]
    public void Parse_SingleValidDirective_ReturnsExpectedCriterion(string input, string expectedProperty, SortDirection expectedDirection)
    {
        var result = _parser.Parse(input);

        var criterion = Assert.Single(result);
        Assert.Equal(expectedProperty, ((ISortCriterion)criterion).Property);
        Assert.Equal(expectedDirection, criterion.Direction);
    }

    [Fact]
    public void Parse_MultipleDirectives_ReturnsAllCriteria()
    {
        var result = _parser.Parse("Name DESC, Price ASC").ToList();

        Assert.Equal(2, result.Count);

        Assert.Equal("Name", ((ISortCriterion)result[0]).Property);
        Assert.Equal(SortDirection.Descending, result[0].Direction);

        Assert.Equal("Price", ((ISortCriterion)result[1]).Property);
        Assert.Equal(SortDirection.Ascending, result[1].Direction);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Parse_NullOrEmpty_ThrowsArgumentException(string? input)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => _parser.Parse(input!).ToList());

        Assert.Equal("sortString", ex.ParamName);
    }

    [Theory]
    [InlineData("  ")]
    [InlineData("Name, , Price")]
    public void Parse_ContainsEmptyDirective_ThrowsFormatException(string input)
    {
        var ex = Assert.Throws<FormatException>(() => _parser.Parse(input).ToList());

        Assert.Contains("empty sorting directive", ex.Message);
    }

    [Fact]
    public void Parse_InvalidSortDirection_ThrowsFormatException()
    {
        var ex = Assert.Throws<FormatException>(() => _parser.Parse("Name DOWN").ToList());

        Assert.Contains("Invalid sort direction", ex.Message);
    }

    [Fact]
    public void Parse_TooManyPartsInDirective_ThrowsFormatException()
    {
        var ex = Assert.Throws<FormatException>(() => _parser.Parse("Name DESC EXTRA").ToList());

        Assert.Contains("Expected format is", ex.Message);
    }

    [Fact]
    public void Parse_InvalidProperty_ThrowsFormatExceptionWithInnerException()
    {
        var ex = Assert.Throws<FormatException>(() => _parser.Parse("NonExistent ASC").ToList());

        Assert.IsType<ArgumentException>(ex.InnerException);
    }

    [Fact]
    public void Parse_CaseInsensitiveDirection_ParsesSuccessfully()
    {
        var result = _parser.Parse("Price desc").ToList();

        var criterion = Assert.Single(result);
        Assert.Equal("Price", ((ISortCriterion)criterion).Property);
        Assert.Equal(SortDirection.Descending, criterion.Direction);
    }
}
