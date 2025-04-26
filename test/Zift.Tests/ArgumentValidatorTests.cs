namespace Zift.Tests;

public class ArgumentValidatorTests
{
    [Fact]
    public void ThrowIfNull_ValueIsNull_ThrowsArgumentNullException()
    {
        object? value = null;

        Assert.Throws<ArgumentNullException>("value", () => ArgumentValidator.ThrowIfNull(value));
    }

    [Theory]
    [MemberData(nameof(NonNullValues))]
    public void ThrowIfNull_ValueIsNotNull_ReturnsValue(object value)
    {
        var result = ArgumentValidator.ThrowIfNull(value);

        Assert.Same(value, result);
    }

    [Fact]
    public void ThrowIfNullOrEmpty_StringValueIsNull_ThrowsArgumentNullException()
    {
        string? value = null;

        Assert.Throws<ArgumentNullException>("value", () => ArgumentValidator.ThrowIfNullOrEmpty(value));
    }

    [Fact]
    public void ThrowIfNullOrEmpty_StringValueIsEmpty_ThrowsArgumentException()
    {
        var value = string.Empty;

        Assert.Throws<ArgumentException>("value", () => ArgumentValidator.ThrowIfNullOrEmpty(value));
    }

    [Fact]
    public void ThrowIfNullOrEmpty_StringValueIsNotNullOrEmpty_ReturnsValue()
    {
        var value = "test";
        var result = ArgumentValidator.ThrowIfNullOrEmpty(value);
        
        Assert.Same(value, result);
    }

    [Fact]
    public void ThrowIfNullOrEmpty_EnumerableValueIsNull_ThrowsArgumentNullException()
    {
        IEnumerable<object>? value = null;
        
        Assert.Throws<ArgumentNullException>("value", () => ArgumentValidator.ThrowIfNullOrEmpty(value));
    }

    [Fact]
    public void ThrowIfNullOrEmpty_EnumerableValueIsEmpty_ThrowsArgumentException()
    {
        var value = Enumerable.Empty<object>();
        
        Assert.Throws<ArgumentException>("value", () => ArgumentValidator.ThrowIfNullOrEmpty(value));
    }

    [Fact]
    public void ThrowIfNullOrEmpty_EnumerableValueIsNotNullOrEmpty_ReturnsValue()
    {
        var value = new object?[] { null };
        var result = ArgumentValidator.ThrowIfNullOrEmpty(value);

        Assert.Same(value, result);
    }

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(1.0, 0.0)]
    [InlineData(1.0, 1.0)]
    [InlineData(2.9, 2.9)]
    [InlineData(3.0, 2.9)]
    public void ThrowIfLessThan_ValueIsGreaterOrEqual_ReturnsValue(double value, double other)
    {
        var result = ArgumentValidator.ThrowIfLessThan(value, other);

        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData(1.0, 1.1)]
    [InlineData(2.9, 3.0)]
    [InlineData(3.0, 3.01)]
    public void ThrowIfLessThan_ValueIsLess_ThrowsArgumentOutOfRangeException(double value, double other)
    {
        Assert.Throws<ArgumentOutOfRangeException>("value", () => ArgumentValidator.ThrowIfLessThan(value, other));
    }

    #region Fixture
    public static IEnumerable<object[]> NonNullValues()
    {
        yield return new object[] { new() };
        yield return new object[] { string.Empty };
        yield return new object[] { Guid.Empty };
        yield return new object[] { 0 };
    }
    #endregion
}
