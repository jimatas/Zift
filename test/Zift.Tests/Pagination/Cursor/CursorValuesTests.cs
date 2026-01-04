namespace Zift.Pagination.Cursor;

using Fixture;

public sealed class CursorValuesTests
{
    [Fact]
    public void EncodeDecode_RoundTripsAllValues()
    {
        var original = new CursorValues(
        [
            123,
            "Alice",
            TestEnum.B,
            null
        ]);

        var cursorValueTypes = new[]
        {
            typeof(int),
            typeof(string),
            typeof(TestEnum),
            typeof(object)
        };

        var encoded = original.Encode();
        var decoded = CursorValues.Decode(encoded, cursorValueTypes);

        Assert.Equal(original.Values.Count, decoded.Values.Count);
        Assert.Equal(123, decoded.Values[0]);
        Assert.Equal("Alice", decoded.Values[1]);
        Assert.Equal(TestEnum.B, decoded.Values[2]);
        Assert.Null(decoded.Values[3]);
    }

    [Fact]
    public void Decode_NullCursor_ThrowsArgumentException()
    {
        Assert.ThrowsAny<ArgumentException>(
            () => CursorValues.Decode(null!, [typeof(int)]));
    }

    [Fact]
    public void Decode_JsonNull_ThrowsFormatException()
    {
        var encoded = Convert.ToBase64String(
            Encoding.UTF8.GetBytes("null"));

        Assert.ThrowsAny<FormatException>(
            () => CursorValues.Decode(encoded, [typeof(int)]));
    }

    [Fact]
    public void Decode_InvalidBase64_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(
            () => CursorValues.Decode("not-base64", [typeof(int)]));
    }

    [Fact]
    public void Decode_MismatchedElementCount_ThrowsFormatException()
    {
        var original = new CursorValues([1, 2]);
        var encoded = original.Encode();

        Assert.Throws<FormatException>(
            () => CursorValues.Decode(encoded, [typeof(int)]));
    }
}
