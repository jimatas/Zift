namespace Zift.Tests;

using Filtering.Dynamic;

public class LiteralValueTests
{
    [Theory]
    [InlineData(null, null, "null")]
    [InlineData(true, null, "true")]
    [InlineData(false, null, "false")]
    [InlineData("abc", null, "abc")]
    [InlineData("abc", StringValueModifier.IgnoreCase, "abc:i")]
    public void ToString_WithVariousValues_ReturnsExpectedString(object? rawValue, StringValueModifier? modifier, string expected)
    {
        var literal = new LiteralValue(rawValue) { Modifier = modifier };

        var result = literal.ToString();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToString_WithDoubleValue_UsesRoundTripFormat()
    {
        var literal = new LiteralValue(1.23456789d);

        var result = literal.ToString();

        Assert.Equal(1.23456789d.ToString("R", CultureInfo.InvariantCulture), result);
    }

    [Fact]
    public void ToString_WithIntValue_UsesDefaultToString()
    {
        var literal = new LiteralValue(42);

        var result = literal.ToString();

        Assert.Equal("42", result);
    }
}
