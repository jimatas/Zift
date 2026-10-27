using Zift.Pagination.Cursor;

namespace Zift.Pagination.Hardening;

public class CursorFingerprintTests
{
    private sealed record Row(int Id, string Name);

    private static readonly Row[] Rows = Enumerable.Range(1, 6).Select(i => new Row(i, "n" + i)).ToArray();

    private sealed record Other(int Id, string Name);

    private static readonly Other[] OtherRows = Enumerable.Range(1, 6).Select(i => new Other(i, "n" + i)).ToArray();

    private const string IdLowerFingerprint = "3AA4DD7A9FFB90AE";
    private const string IdNameLowerFingerprint = "9349572520F87FB5";
    private const string IdHigherFingerprint = "D55B6143E553321C";
    private const string IdNameHigherFingerprint = "F44F548F04381BC1";
    private const string IdLowerAfter3Cursor = "eyJmIjoiM0FBNEREN0E5RkZCOTBBRSIsInYiOlszXX0";

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
    public void Fingerprint_Is64BitsOfHex()
    {
        var fingerprint = FingerprintOf(EndCursor(null, composite: false));

        Assert.Equal(16, fingerprint.Length);
        Assert.All(fingerprint, c => Assert.True(char.IsAsciiHexDigitUpper(c) || char.IsAsciiDigit(c)));
    }

    [Fact]
    public void SameOrderingOnDifferentEntityTypes_ProducesDifferentFingerprints()
    {
        var rowCursor = Rows.AsQueryable().AsCursorQuery().OrderBy(r => r.Id).ToCursorPage(2).EndCursor;
        var otherCursor = OtherRows.AsQueryable().AsCursorQuery().OrderBy(o => o.Id).ToCursorPage(2).EndCursor;

        Assert.NotEqual(FingerprintOf(rowCursor), FingerprintOf(otherCursor));
    }

    [Fact]
    public void CursorFromAnotherEntityType_IsRejected()
    {
        var rowCursor = Rows.AsQueryable().AsCursorQuery().OrderBy(r => r.Id).ToCursorPage(2).EndCursor;

        var error = Assert.Throws<InvalidCursorException>(() =>
            OtherRows.AsQueryable().AsCursorQuery().OrderBy(o => o.Id).After(rowCursor).ToCursorPage(2));

        Assert.Equal("The cursor was created for a different ordering or entity type.", error.Message);
    }

    [Fact]
    public void CursorFromAnotherEntityType_IsRejectedWhenNavigatingBackward()
    {
        var rowCursor = Rows.AsQueryable().AsCursorQuery().OrderBy(r => r.Id).ToCursorPage(2).EndCursor;

        Assert.Throws<InvalidCursorException>(() =>
            OtherRows.AsQueryable().AsCursorQuery().OrderBy(o => o.Id).Before(rowCursor).ToCursorPage(2));
    }

    [Fact]
    public void CursorWithThePinnedFormat_IsAccepted()
    {
        var page = Rows.AsQueryable().AsCursorQuery().OrderBy(r => r.Id).After(IdLowerAfter3Cursor).ToCursorPage(2);

        Assert.Equal([4, 5], page.Items.Select(r => r.Id));
    }
}
