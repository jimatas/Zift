namespace Zift.Tests;

using Filtering.Dynamic;
using Filtering.Dynamic.Parsing;

public class SyntaxTokenExtensionsTests
{
    [Theory]
    [InlineData("&&", LogicalOperator.And)]
    [InlineData("||", LogicalOperator.Or)]
    public void ToLogicalOperator_ForSupportedOperator_ReturnsExpectedLogicalOperator(string @operator, LogicalOperator expectedResult)
    {
        var token = new SyntaxToken(SyntaxTokenType.LogicalOperator, @operator, Position: 0);

        var result = token.ToLogicalOperator();

        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void ToLogicalOperator_ForInvalidOperator_ThrowsSyntaxErrorException()
    {
        var token = new SyntaxToken(SyntaxTokenType.LogicalOperator, "invalid", Position: 0);

        var ex = Assert.Throws<SyntaxErrorException>(() => token.ToLogicalOperator());

        Assert.StartsWith("Expected a logical operator, but got: invalid", ex.Message);
    }

    [Theory]
    [InlineData("==", ComparisonOperator.Equal)]
    [InlineData("!=", ComparisonOperator.NotEqual)]
    [InlineData(">", ComparisonOperator.GreaterThan)]
    [InlineData(">=", ComparisonOperator.GreaterThanOrEqual)]
    [InlineData("<", ComparisonOperator.LessThan)]
    [InlineData("<=", ComparisonOperator.LessThanOrEqual)]
    [InlineData("%=", ComparisonOperator.Contains)]
    [InlineData("^=", ComparisonOperator.StartsWith)]
    [InlineData("$=", ComparisonOperator.EndsWith)]
    public void ToComparisonOperator_ForSupportedOperator_ReturnsExpectedComparisonOperator(string @operator, ComparisonOperator expectedResult)
    {
        var token = new SyntaxToken(SyntaxTokenType.ComparisonOperator, @operator, Position: 0);

        var result = token.ToComparisonOperator();

        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void ToComparisonOperator_ForInvalidOperator_ThrowsSyntaxErrorException()
    {
        var token = new SyntaxToken(SyntaxTokenType.ComparisonOperator, "invalid", Position: 0);

        var ex = Assert.Throws<SyntaxErrorException>(() => token.ToComparisonOperator());

        Assert.StartsWith("Expected a comparison operator, but got: invalid", ex.Message);
    }

    [Fact]
    public void ToLogicalOperator_WithNullValue_ThrowsSyntaxErrorException()
    {
        var token = new SyntaxToken(SyntaxTokenType.LogicalOperator, null!, Position: 0);

        var ex = Assert.Throws<SyntaxErrorException>(() => token.ToLogicalOperator());

        Assert.StartsWith("Expected a logical operator, but got: ", ex.Message);
    }
}
