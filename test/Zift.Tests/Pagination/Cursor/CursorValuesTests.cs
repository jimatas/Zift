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
    public void Decode_JsonNull_ThrowsFormatException()
    {
        var encoded = Convert.ToBase64String(
            Encoding.UTF8.GetBytes("null"));

        var ex = Assert.ThrowsAny<FormatException>(() =>
            CursorValues.Decode(encoded, [typeof(int)]));

        var inner = Assert.IsType<FormatException>(ex.InnerException);
        Assert.Contains("Cursor is not a valid JSON array.", inner.Message);
    }

    [Fact]
    public void Decode_InvalidBase64_ThrowsFormatException()
    {
        var ex = Assert.Throws<FormatException>(() =>
            CursorValues.Decode("not-base64", [typeof(int)]));

        Assert.IsType<FormatException>(ex.InnerException);
    }

    [Fact]
    public void Decode_MismatchedElementCount_ThrowsFormatException()
    {
        var original = new CursorValues([1, 2]);
        var encoded = original.Encode();

        var ex = Assert.Throws<FormatException>(() =>
            CursorValues.Decode(encoded, [typeof(int)]));

        var inner = Assert.IsType<FormatException>(ex.InnerException);
        Assert.Contains("Cursor contains 2 element(s), expected 1.", inner.Message);
    }
}
