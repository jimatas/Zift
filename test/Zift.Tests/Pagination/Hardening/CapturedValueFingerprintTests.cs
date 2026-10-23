using Zift.Pagination.Cursor;

namespace Zift.Pagination.Tests;

public class CapturedValueFingerprintTests
{
    private sealed record Row(int Id, string Name);

    private static readonly IQueryable<Row> Rows = Enumerable.Range(1, 6)
        .Select(i => new Row(i, "n" + i))
        .AsQueryable();

    private sealed class Scale(int factor)
    {
        public int Factor { get; } = factor;

        public int Field = factor;
    }

    private static IOrderedCursorQuery<Row> ScaledById(int sign)
    {
        return Rows.AsCursorQuery().OrderBy(r => r.Id * sign);
    }

    private static IOrderedCursorQuery<Row> ScaledByProperty(Scale scale)
    {
        return Rows.AsCursorQuery().OrderBy(r => r.Id * scale.Factor);
    }

    private static IOrderedCursorQuery<Row> ScaledByField(Scale scale)
    {
        return Rows.AsCursorQuery().OrderBy(r => r.Id * scale.Field);
    }

    private static IOrderedCursorQuery<Row> Prefixed(string prefix)
    {
        return Rows.AsCursorQuery().OrderBy(r => prefix + r.Name);
    }

    [Fact]
    public void CursorFromAnotherCapturedValue_IsRejected()
    {
        var cursor = ScaledById(sign: 1).ToCursorPage(2).EndCursor;

        var error = Assert.Throws<InvalidCursorException>(() =>
            ScaledById(sign: -1).After(cursor).ToCursorPage(2));

        Assert.Equal("The cursor was created for a different ordering or entity type.", error.Message);
    }

    [Fact]
    public void CursorFromAnotherCapturedValue_IsRejectedWhenNavigatingBackward()
    {
        var cursor = ScaledById(sign: 1).ToCursorPage(2).EndCursor;

        Assert.Throws<InvalidCursorException>(() =>
            ScaledById(sign: -1).Before(cursor).ToCursorPage(2));
    }

    [Fact]
    public void CursorFromTheSameCapturedValue_IsAcceptedAcrossSeparateClosures()
    {
        var cursor = ScaledById(sign: 1).ToCursorPage(2).EndCursor;

        var page = ScaledById(sign: 1).After(cursor).ToCursorPage(2);

        Assert.Equal([3, 4], page.Items.Select(r => r.Id));
    }

    [Fact]
    public void CursorFromAnotherCapturedProperty_IsRejected()
    {
        var cursor = ScaledByProperty(new Scale(1)).ToCursorPage(2).EndCursor;

        Assert.Throws<InvalidCursorException>(() =>
            ScaledByProperty(new Scale(-1)).After(cursor).ToCursorPage(2));
    }

    [Fact]
    public void CursorFromAnotherCapturedField_IsRejected()
    {
        var cursor = ScaledByField(new Scale(1)).ToCursorPage(2).EndCursor;

        Assert.Throws<InvalidCursorException>(() =>
            ScaledByField(new Scale(-1)).After(cursor).ToCursorPage(2));
    }

    [Fact]
    public void CursorFromAnotherCapturedString_IsRejected()
    {
        var cursor = Prefixed("n").ToCursorPage(2).EndCursor;

        Assert.Throws<InvalidCursorException>(() =>
            Prefixed("x").After(cursor).ToCursorPage(2));
    }

    [Fact]
    public void CursorFromTheSameCapturedProperty_IsAcceptedAcrossSeparateInstances()
    {
        var cursor = ScaledByProperty(new Scale(1)).ToCursorPage(2).EndCursor;

        var page = ScaledByProperty(new Scale(1)).After(cursor).ToCursorPage(2);

        Assert.Equal([3, 4], page.Items.Select(r => r.Id));
    }

    [Fact]
    public void CursorWithoutCapturedValues_IsUnaffected()
    {
        var cursor = Rows.AsCursorQuery().OrderBy(r => r.Id).ToCursorPage(2).EndCursor;

        var page = Rows.AsCursorQuery().OrderBy(r => r.Id).After(cursor).ToCursorPage(2);

        Assert.Equal([3, 4], page.Items.Select(r => r.Id));
    }
}
