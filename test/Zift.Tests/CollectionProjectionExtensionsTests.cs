namespace Zift.Tests;

using Filtering.Dynamic;

public class CollectionProjectionExtensionsTests
{
    [Theory]
    [InlineData(CollectionProjection.Count, "count")]
    public void ToSymbol_KnownProjection_ReturnsExpectedSymbol(CollectionProjection projection, string expected)
    {
        var result = projection.ToSymbol();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToSymbol_UnknownProjection_FallsBackToEnumName()
    {
        var unknown = (CollectionProjection)99;

        var result = unknown.ToSymbol();

        Assert.Equal("99", result);
    }

    [Theory]
    [InlineData("count", CollectionProjection.Count)]
    [InlineData("Count", CollectionProjection.Count)]
    [InlineData("COUNT", CollectionProjection.Count)]
    public void FromSymbol_ValidSymbol_ReturnsExpectedProjection(string symbol, CollectionProjection expected)
    {
        var result = CollectionProjectionExtensions.FromSymbol(symbol);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FromSymbol_InvalidSymbol_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            CollectionProjectionExtensions.FromSymbol("unknown"));

        Assert.Contains("unknown", ex.Message);
    }
}
