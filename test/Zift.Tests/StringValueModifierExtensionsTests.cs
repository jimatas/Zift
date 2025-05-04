namespace Zift.Tests;

using Filtering.Dynamic;

public class StringValueModifierExtensionsTests
{
    [Theory]
    [InlineData(StringValueModifier.IgnoreCase, "i")]
    public void ToDisplayString_KnownModifier_ReturnsExpectedSymbol(StringValueModifier modifier, string expected)
    {
        var symbol = modifier.ToDisplayString();

        Assert.Equal(expected, symbol);
    }

    [Fact]
    public void ToDisplayString_UnknownModifier_FallsBackToEnumValue()
    {
        var unknown = (StringValueModifier)99;
        var result = unknown.ToDisplayString();

        Assert.Equal("99", result);
    }

    [Theory]
    [InlineData("i", StringValueModifier.IgnoreCase)]
    [InlineData("I", StringValueModifier.IgnoreCase)]
    public void TryParse_ValidSymbol_ReturnsExpectedModifier(string symbol, StringValueModifier expected)
    {
        var success = StringValueModifierExtensions.TryParse(symbol, out var actual);

        Assert.True(success);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("x")]
    [InlineData("ignoreCase")]
    [InlineData("")]
    public void TryParse_InvalidSymbol_ReturnsFalse(string symbol)
    {
        var success = StringValueModifierExtensions.TryParse(symbol, out var actual);

        Assert.False(success);
    }
}
