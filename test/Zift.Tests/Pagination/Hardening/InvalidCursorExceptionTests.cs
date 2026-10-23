using System.Text;
using System.Text.Json.Nodes;
using Zift.Pagination.Cursor;

namespace Zift.Pagination.Tests;

public class InvalidCursorExceptionTests
{
    private sealed record Row(int Id, string Name);

    private sealed record VersionRow(int Id, Version Version);

    private sealed class Version : IComparable<Version>
    {
        public Version(int major)
            : this(major, 0)
        {
        }

        public Version(int major, int minor)
        {
            Major = major;
            Minor = minor;
        }

        public int Major { get; }

        public int Minor { get; }

        public int CompareTo(Version? other)
        {
            return Major.CompareTo(other?.Major ?? 0);
        }
    }

    private static IQueryable<Row> Source { get; } =
        Enumerable.Range(1, 5).Select(i => new Row(i, $"N{i}")).AsQueryable();

    private static IOrderedCursorQuery<Row> ById => Source.AsCursorQuery().OrderBy(r => r.Id);

    private static string Encode(JsonNode payload)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(payload.ToJsonString()))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static JsonObject Decode(string cursor)
    {
        var base64 = cursor.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');

        return JsonNode.Parse(Convert.FromBase64String(base64))!.AsObject();
    }

    [Fact]
    public void InvalidCursorException_IsAFormatException()
    {
        Assert.True(typeof(FormatException).IsAssignableFrom(typeof(InvalidCursorException)));
        Assert.Throws<InvalidCursorException>(() => ById.After("garbage").ToCursorPage(2));
    }

    [Fact]
    public void MalformedCursor_KeepsTheDecodingFailureAsInnerException()
    {
        var error = Assert.Throws<InvalidCursorException>(() => ById.After("garbage").ToCursorPage(2));

        Assert.Equal("The cursor is malformed.", error.Message);
        Assert.NotNull(error.InnerException);
    }

    [Theory]
    [InlineData("e30")]
    [InlineData("bnVsbA")]
    public void IncompleteCursor_ReportsIncomplete_RatherThanMalformed(string cursor)
    {
        var error = Assert.Throws<InvalidCursorException>(() => ById.After(cursor).ToCursorPage(2));

        Assert.Equal("The cursor is incomplete.", error.Message);
        Assert.Null(error.InnerException);
    }

    [Fact]
    public void CursorFromDifferentOrdering_Throws()
    {
        var cursor = ById.ToCursorPage(2).EndCursor;

        var error = Assert.Throws<InvalidCursorException>(() =>
            Source.AsCursorQuery().OrderBy(r => r.Name).After(cursor).ToCursorPage(2));
        Assert.Equal("The cursor was created for a different ordering or entity type.", error.Message);
    }

    [Fact]
    public void CursorWithWrongKeyCount_Throws()
    {
        var payload = Decode(ById.ToCursorPage(2).EndCursor!);
        payload["v"] = new JsonArray();

        Assert.Throws<InvalidCursorException>(() => ById.After(Encode(payload)).ToCursorPage(2));
    }

    [Fact]
    public void CursorWithMalformedKeyValue_KeepsInnerException()
    {
        var payload = Decode(ById.ToCursorPage(2).EndCursor!);
        payload["v"] = new JsonArray("not a number");

        var error = Assert.Throws<InvalidCursorException>(() => ById.After(Encode(payload)).ToCursorPage(2));
        Assert.NotNull(error.InnerException);
    }

    [Fact]
    public void CursorWithNullForNonNullableKey_Throws()
    {
        var payload = Decode(ById.ToCursorPage(2).EndCursor!);
        payload["v"] = new JsonArray((JsonNode?)null);

        Assert.Throws<InvalidCursorException>(() => ById.After(Encode(payload)).ToCursorPage(2));
    }

    [Fact]
    public void KeyTypeThatCannotBeDeserialized_PropagatesNotSupportedException()
    {
        var rows = Enumerable.Range(1, 5).Select(i => new VersionRow(i, new Version(i))).AsQueryable();
        var query = rows.AsCursorQuery().OrderBy(r => r.Version).ThenBy(r => r.Id);
        var cursor = query.ToCursorPage(2).EndCursor;

        Assert.Throws<NotSupportedException>(() => query.After(cursor).ToCursorPage(2));
    }

    [Fact]
    public void OtherUsageErrors_AreNotInvalidCursorExceptions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ById.ToCursorPage(0));
    }
}
