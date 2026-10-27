using Zift.Pagination.Cursor;

namespace Zift.Pagination.Hardening;

public class CultureFingerprintTests
{
    private sealed record Row(int Id, string Name);

    private static readonly IQueryable<Row> Rows = Enumerable.Range(1, 6)
        .Select(i => new Row(i, "n" + i))
        .AsQueryable();

    private static IOrderedCursorQuery<Row> ScaledInline()
    {
        return Rows.AsCursorQuery().OrderBy(r => (decimal)r.Id * 1.5m);
    }

    private static IOrderedCursorQuery<Row> ScaledByCaptured(double rate)
    {
        return Rows.AsCursorQuery().OrderBy(r => r.Id * rate);
    }

    private static T Under<T>(string culture, Func<T> action)
    {
        var original = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);

            return action();
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Theory]
    [InlineData("en-US", "de-DE")]
    [InlineData("de-DE", "en-US")]
    [InlineData("en-US", "fr-FR")]
    [InlineData("sv-SE", "en-US")]
    public void CursorWithInlineDecimalConstant_IsAcceptedUnderAnotherCulture(string issuedUnder, string usedUnder)
    {
        var cursor = Under(issuedUnder, () => ScaledInline().ToCursorPage(2).EndCursor);

        var page = Under(usedUnder, () => ScaledInline().After(cursor).ToCursorPage(2));

        Assert.Equal([3, 4], page.Items.Select(r => r.Id));
    }

    [Theory]
    [InlineData("en-US", "de-DE")]
    [InlineData("de-DE", "en-US")]
    [InlineData("sv-SE", "en-US")]
    public void CursorWithCapturedDouble_IsAcceptedUnderAnotherCulture(string issuedUnder, string usedUnder)
    {
        var cursor = Under(issuedUnder, () => ScaledByCaptured(0.5).ToCursorPage(2).EndCursor);

        var page = Under(usedUnder, () => ScaledByCaptured(0.5).After(cursor).ToCursorPage(2));

        Assert.Equal([3, 4], page.Items.Select(r => r.Id));
    }
}
