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

    [Fact]
    public void IsTerminal_Count_ReturnsTrue()
    {
        var result = CollectionProjection.Count.IsTerminal();

        Assert.True(result);
    }

    [Fact]
    public void IsTerminal_UnknownProjection_ReturnsFalse()
    {
        var unknown = (CollectionProjection)99;

        var result = unknown.IsTerminal();

        Assert.False(result);
    }

    [Fact]
    public void GetLinqMethod_Count_ReturnsExpectedMethod()
    {
        var method = CollectionProjection.Count.GetLinqMethod();

        Assert.Equal("Count", method.Name);
        Assert.Single(method.GetParameters());
        Assert.True(typeof(IEnumerable<>).IsAssignableFrom(method.GetParameters()[0].ParameterType.GetGenericTypeDefinition()));
    }

    [Fact]
    public void GetLinqMethod_UnknownProjection_ThrowsNotSupportedException()
    {
        var unknown = (CollectionProjection)99;

        var ex = Assert.Throws<NotSupportedException>(() => unknown.GetLinqMethod());

        Assert.Contains("LINQ method not defined for collection projection", ex.Message);
    }
}
