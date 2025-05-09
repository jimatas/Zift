namespace Zift.Tests;

using Filtering.Dynamic;
using Filtering.Dynamic.Parsing;
using Fixture;

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

    [Fact]
    public void ToLogicalOperator_WithNullValue_ThrowsSyntaxErrorException()
    {
        var token = new SyntaxToken(SyntaxTokenType.LogicalOperator, null!, Position: 0);

        var ex = Assert.Throws<SyntaxErrorException>(() => token.ToLogicalOperator());

        Assert.StartsWith("Expected a logical operator, but got: ", ex.Message);
    }

    [Theory]
    [ClassData(typeof(ComparisonOperatorData))]
    public void ToComparisonOperator_ForSupportedOperator_ReturnsExpectedComparisonOperator(string @operator, ComparisonOperatorType expectedResult)
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

    [Theory]
    [InlineData("42")]
    [InlineData("3.14")]
    [InlineData("-42")]
    [InlineData("-3.14")]
    [InlineData("2147483647")]
    [InlineData("-2147483648")]
    [InlineData("1.7976931348623157E+308")]
    [InlineData("-1.7976931348623157E+308")]
    public void ToTypedValue_WithValidNumericLiteral_ReturnsExpectedValue(string literal)
    {
        var result = new SyntaxToken(SyntaxTokenType.NumericLiteral, literal, 0).ToTypedValue();

        Assert.Equal(literal, string.Format(CultureInfo.InvariantCulture, "{0}", result));
    }

    [Theory]
    [InlineData("42_000")]
    [InlineData("3.14_15")]
    [InlineData("0xFF")]
    public void ToTypedValue_WithInvalidNumericLiteral_ThrowsSyntaxErrorException(string literal)
    {
        var token = new SyntaxToken(SyntaxTokenType.NumericLiteral, literal, 0);

        Assert.Throws<SyntaxErrorException>(() => _ = token.ToTypedValue());
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("false", false)]
    [InlineData("False", false)]
    [InlineData("null", null)]
    [InlineData("NULL", null)]
    public void ToTypedValue_WithValidKeyword_ReturnsExpectedValue(string keyword, object? expected)
    {
        var result = new SyntaxToken(SyntaxTokenType.Keyword, keyword, 0).ToTypedValue();

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("nill")]
    [InlineData("void")]
    [InlineData("undefined")]
    public void ToTypedValue_WithInvalidKeyword_ThrowsSyntaxErrorException(string keyword)
    {
        var token = new SyntaxToken(SyntaxTokenType.Keyword, keyword, 0);

        Assert.Throws<SyntaxErrorException>(() => _ = token.ToTypedValue());
    }

    [Theory]
    [InlineData("\"\"")]
    [InlineData("''")]
    [InlineData("\"John Doe\"")]
    [InlineData("'Jane Smith'")]
    public void ToTypedValue_WithProperlyQuotedStringLiteral_ReturnsExpectedValue(string literal)
    {
        var result = new SyntaxToken(SyntaxTokenType.StringLiteral, literal, 0).ToTypedValue();

        Assert.Equal(literal[1..^1], result);
    }

    [Theory]
    [InlineData("\"")]
    [InlineData("'")]
    [InlineData("\"'")]
    [InlineData("'\"")]
    [InlineData("John Doe")]
    public void ToTypedValue_WithImproperlyQuotedStringLiteral_ReturnsStringAsIs(string literal)
    {
        var result = new SyntaxToken(SyntaxTokenType.StringLiteral, literal, 0).ToTypedValue();

        Assert.Equal(literal, result);
    }

    [Theory]
    [InlineData("\"\\\"\"", "\"")]
    [InlineData("'\\\"'", "\\\"")]
    [InlineData("\"\\'\"", "\\'")]
    [InlineData("'\\''", "'")]
    public void ToTypedValue_WithEscapedStringLiteral_ReturnsExpectedValue(string literal, string expected)
    {
        var result = new SyntaxToken(SyntaxTokenType.StringLiteral, literal, 0).ToTypedValue();

        Assert.Equal(expected, result);
    }
}
