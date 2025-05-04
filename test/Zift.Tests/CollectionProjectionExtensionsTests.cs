namespace Zift.Tests;

using Filtering.Dynamic;

public class CollectionProjectionExtensionsTests
{
    [Theory]
    [InlineData(CollectionProjection.Count, "count")]
    public void ToDisplayString_KnownProjection_ReturnsExpectedSymbol(CollectionProjection projection, string expected)
    {
        var result = projection.ToDisplayString();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToDisplayString_UnknownProjection_FallsBackToEnumName()
    {
        var unknown = (CollectionProjection)99;
        var result = unknown.ToDisplayString();

        Assert.Equal("99", result);
    }

    [Theory]
    [InlineData("count", CollectionProjection.Count)]
    [InlineData("COUNT", CollectionProjection.Count)]
    [InlineData("Count", CollectionProjection.Count)]
    public void TryParse_ValidSymbol_ReturnsExpectedProjection(string symbol, CollectionProjection expected)
    {
        var success = CollectionProjectionExtensions.TryParse(symbol, out var actual);

        Assert.True(success);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("cnt")]
    public void TryParse_InvalidSymbol_ReturnsFalse(string symbol)
    {
        var success = CollectionProjectionExtensions.TryParse(symbol, out var actual);

        Assert.False(success);
        Assert.Equal(default, actual);
    }
}
