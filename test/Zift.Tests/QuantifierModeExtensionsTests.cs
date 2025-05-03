namespace Zift.Tests;

using Filtering.Dynamic;

public class QuantifierModeExtensionsTests
{
    [Theory]
    [InlineData(QuantifierMode.Any, "any")]
    [InlineData(QuantifierMode.All, "all")]
    public void ToSymbol_KnownQuantifier_ReturnsExpectedSymbol(QuantifierMode mode, string expected)
    {
        var result = mode.ToSymbol();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToSymbol_UnknownQuantifier_FallsBackToEnumName()
    {
        var unknown = (QuantifierMode)99;

        var result = unknown.ToSymbol();

        Assert.Equal("99", result);
    }

    [Theory]
    [InlineData("any", QuantifierMode.Any)]
    [InlineData("Any", QuantifierMode.Any)]
    [InlineData("ANY", QuantifierMode.Any)]
    [InlineData("all", QuantifierMode.All)]
    [InlineData("All", QuantifierMode.All)]
    [InlineData("ALL", QuantifierMode.All)]
    public void FromSymbol_ValidSymbol_ReturnsExpectedMode(string symbol, QuantifierMode expected)
    {
        var result = QuantifierModeExtensions.FromSymbol(symbol);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FromSymbol_InvalidSymbol_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            QuantifierModeExtensions.FromSymbol("invalid"));

        Assert.Contains("invalid", ex.Message);
    }
}
