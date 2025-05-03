namespace Zift.Tests;

using Filtering.Dynamic;

public class StringValueModifierExtensionsTests
{
    [Theory]
    [InlineData(StringValueModifier.IgnoreCase, "i")]
    public void ToSymbol_KnownModifier_ReturnsExpectedSymbol(StringValueModifier modifier, string expected)
    {
        var symbol = modifier.ToSymbol();

        Assert.Equal(expected, symbol);
    }

    [Fact]
    public void ToSymbol_UnknownModifier_FallsBackToEnumName()
    {
        var unknown = (StringValueModifier)99;

        var result = unknown.ToSymbol();

        Assert.Equal("99", result);
    }

    [Theory]
    [InlineData("i", StringValueModifier.IgnoreCase)]
    [InlineData("I", StringValueModifier.IgnoreCase)]
    public void FromSymbol_ValidSymbol_ReturnsExpectedModifier(string symbol, StringValueModifier expected)
    {
        var result = StringValueModifierExtensions.FromSymbol(symbol);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("x")]
    [InlineData("ignoreCase")]
    [InlineData("")]
    public void FromSymbol_InvalidSymbol_ThrowsArgumentException(string symbol)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            StringValueModifierExtensions.FromSymbol(symbol));

        Assert.StartsWith("Unknown string modifier", ex.Message);
    }
}
