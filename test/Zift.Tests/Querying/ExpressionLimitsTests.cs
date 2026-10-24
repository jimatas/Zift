namespace Zift.Querying;

using ExpressionBuilding;
using Fixture;
using Parsing;

public sealed class ExpressionLimitsTests
{
    private static IQueryable<TestClass> Source => new[] { new TestClass { Int32Value = 1 } }.AsQueryable();

    [Fact]
    public void Where_ExpressionAtMaxLength_Succeeds()
    {
        const string text = "Int32Value == 1";
        var options = new ExpressionBuilderOptions { MaxExpressionLength = text.Length };

        Assert.Single(Source.Where(text, options));
    }

    [Fact]
    public void Where_ExpressionBeyondMaxLength_ThrowsInvalidFilterException()
    {
        var options = new ExpressionBuilderOptions { MaxExpressionLength = 10 };

        var ex = Assert.Throws<InvalidFilterException>(() =>
            Source.Where("Int32Value == 1", options));

        Assert.Contains("maximum of 10 characters", ex.Message);
    }

    [Fact]
    public void Where_ExpressionBeyondDefaultMaxLength_ThrowsInvalidFilterException()
    {
        var text = "Int32Value == 1" + string.Concat(Enumerable.Repeat(" || Int32Value == 1", 300));

        Assert.Throws<InvalidFilterException>(() => Source.Where(text));
    }

    [Fact]
    public void Where_DeepNestingWithDefaultOptions_ThrowsInsteadOfOverflowingStack()
    {
        var text = new string('!', 2_000) + "(Int32Value == 1)";

        Assert.Throws<SyntaxErrorException>(() => Source.Where(text));
    }

    [Fact]
    public void Where_ListBeyondDefaultMaxListItems_ThrowsSyntaxErrorException()
    {
        var items = string.Join(", ", Enumerable.Range(0, 1_001));
        var options = new ExpressionBuilderOptions { MaxExpressionLength = int.MaxValue };

        Assert.Throws<SyntaxErrorException>(() =>
            Source.Where($"Int32Value in [{items}]", options));
    }

    [Fact]
    public void Where_RaisedLimits_AllowLargerExpressions()
    {
        var options = new ExpressionBuilderOptions
        {
            MaxNestingDepth = 200,
            MaxListItems = 5_000,
            MaxExpressionLength = 100_000
        };

        var nested = new string('(', 150) + "Int32Value == 1" + new string(')', 150);
        var items = string.Join(", ", Enumerable.Range(0, 2_000));

        Assert.Single(Source.Where(nested, options));
        Assert.Single(Source.Where($"Int32Value in [{items}]", options));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Options_NonPositiveLimit_ThrowsArgumentOutOfRangeException(int value)
    {
        var options = new ExpressionBuilderOptions();

        Assert.Throws<ArgumentOutOfRangeException>(() => options.MaxExpressionLength = value);
        Assert.Throws<ArgumentOutOfRangeException>(() => options.MaxNestingDepth = value);
        Assert.Throws<ArgumentOutOfRangeException>(() => options.MaxListItems = value);
    }

    [Fact]
    public void Options_Defaults_AreSafeValues()
    {
        var options = new ExpressionBuilderOptions();

        Assert.Equal(ExpressionBuilderOptions.DefaultMaxExpressionLength, options.MaxExpressionLength);
        Assert.Equal(ExpressionBuilderOptions.DefaultMaxNestingDepth, options.MaxNestingDepth);
        Assert.Equal(ExpressionBuilderOptions.DefaultMaxListItems, options.MaxListItems);
    }
}
