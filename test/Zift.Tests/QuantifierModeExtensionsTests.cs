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
    public void TryParse_ValidSymbol_ReturnsExpectedMode(string symbol, QuantifierMode expected)
    {
        var success = QuantifierModeExtensions.TryParse(symbol, out var actual);

        Assert.True(success);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData("none")]
    public void TryParse_InvalidSymbol_ReturnsFalse(string symbol)
    {
        var success = QuantifierModeExtensions.TryParse(symbol, out var _);

        Assert.False(success);
    }

    [Theory]
    [InlineData(QuantifierMode.Any, false, "Any", 1)]
    [InlineData(QuantifierMode.Any, true, "Any", 2)]
    [InlineData(QuantifierMode.All, true, "All", 2)]
    public void GetLinqMethod_KnownCombination_ReturnsCorrectMethod(QuantifierMode mode, bool withPredicate, string expectedName, int expectedParameterCount)
    {
        var method = mode.GetLinqMethod(withPredicate);

        Assert.Equal(expectedName, method.Name);
        Assert.Equal(expectedParameterCount, method.GetParameters().Length);
    }

    [Fact]
    public void GetLinqMethod_UnknownQuantifier_ThrowsNotSupportedException()
    {
        var unknown = (QuantifierMode)99;

        var ex = Assert.Throws<NotSupportedException>(() => unknown.GetLinqMethod(withPredicate: true));

        Assert.Contains("quantifier mode", ex.Message);
    }

    [Fact]
    public void GetLinqMethod_AllWithoutPredicate_ThrowsNotSupportedException()
    {
        var ex = Assert.Throws<NotSupportedException>(() => QuantifierMode.All.GetLinqMethod(withPredicate: false));

        Assert.Contains("quantifier mode", ex.Message);
    }
}
