using System.Text;
using System.Text.Json;
using Zift.Pagination.Cursor;

namespace Zift.Pagination.Tests;

public class CursorFingerprintTests
{
    private sealed record Row(int Id, string Name);

    private static readonly Row[] Rows = Enumerable.Range(1, 6).Select(i => new Row(i, "n" + i)).ToArray();

    private const string IdLowerFingerprint = "B0284F5A";
    private const string IdNameLowerFingerprint = "32292430";
    private const string IdHigherFingerprint = "37AD89CB";
    private const string IdNameHigherFingerprint = "92CD61A4";
    private const string IdLowerAfter3Cursor = "eyJmIjoiQjAyODRGNUEiLCJ2IjpbM119";

    private static CursorQueryOptions Options(NullOrdering nullOrdering) => new() { NullOrdering = nullOrdering };

    private static string FingerprintOf(string? cursor)
    {
        var padded = cursor!.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
        using var document = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(padded)));

        return document.RootElement.GetProperty("f").GetString()!;
    }

    private static string? EndCursor(CursorQueryOptions? options, bool composite)
    {
        var query = Rows.AsQueryable().AsCursorQuery(options);

        return (composite
            ? query.OrderBy(r => r.Id).ThenByDescending(r => r.Name)
            : query.OrderBy(r => r.Id)).ToCursorPage(2).EndCursor;
    }

    [Theory]
    [InlineData(null)]
    [InlineData(NullOrdering.NullsCompareLower)]
    public void DefaultConvention_PinsTheFingerprint(NullOrdering? nullOrdering)
    {
        var options = nullOrdering is null ? null : Options(nullOrdering.Value);

        Assert.Equal(IdLowerFingerprint, FingerprintOf(EndCursor(options, composite: false)));
        Assert.Equal(IdNameLowerFingerprint, FingerprintOf(EndCursor(options, composite: true)));
    }

    [Fact]
    public void HigherConvention_PinsTheFingerprint()
    {
        var options = Options(NullOrdering.NullsCompareHigher);

        Assert.Equal(IdHigherFingerprint, FingerprintOf(EndCursor(options, composite: false)));
        Assert.Equal(IdNameHigherFingerprint, FingerprintOf(EndCursor(options, composite: true)));
    }

    [Fact]
    public void ConventionsProduceDifferentFingerprints()
    {
        Assert.NotEqual(IdLowerFingerprint, IdHigherFingerprint);
        Assert.NotEqual(IdNameLowerFingerprint, IdNameHigherFingerprint);
    }

    [Fact]
    public void CursorWithThePinnedFormat_IsAccepted()
    {
        var page = Rows.AsQueryable().AsCursorQuery().OrderBy(r => r.Id).After(IdLowerAfter3Cursor).ToCursorPage(2);

        Assert.Equal([4, 5], page.Items.Select(r => r.Id));
    }
}
