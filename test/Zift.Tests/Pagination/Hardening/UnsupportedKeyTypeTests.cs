using Zift.Pagination.Cursor;

namespace Zift.Pagination.Tests;

public class UnsupportedKeyTypeTests
{
    public sealed record Grade(int Level);
    public sealed record Item(int Id, Grade Grade);

    private static IQueryable<Item> Items => new[] { new Item(1, new Grade(3)) }.AsQueryable();

    [Fact]
    public void ToCursorPage_FirstPageOrderedByTypeWithoutComparison_Succeeds()
    {
        var page = Items.AsCursorQuery().OrderBy(i => i.Grade).ToCursorPage(pageSize: 5);

        Assert.Single(page.Items);
        Assert.NotNull(page.EndCursor);
    }

    [Fact]
    public void ToCursorPage_NavigatingByTypeWithoutComparison_ThrowsNotSupportedException()
    {
        var first = Items.AsCursorQuery().OrderBy(i => i.Grade).ToCursorPage(pageSize: 5);

        var ex = Assert.Throws<NotSupportedException>(() =>
            Items.AsCursorQuery()
                .OrderBy(i => i.Grade)
                .After(first.EndCursor)
                .ToCursorPage(pageSize: 5));

        Assert.Contains(nameof(Grade), ex.Message);
        Assert.Contains("not supported for cursor pagination", ex.Message);
    }

    [Fact]
    public void ToCursorPage_NavigatingBackwardByTypeWithoutComparison_ThrowsNotSupportedException()
    {
        var first = Items.AsCursorQuery().OrderBy(i => i.Grade).ToCursorPage(pageSize: 5);

        Assert.Throws<NotSupportedException>(() =>
            Items.AsCursorQuery()
                .OrderBy(i => i.Grade)
                .Before(first.StartCursor)
                .ToCursorPage(pageSize: 5));
    }

    [Fact]
    public void ToCursorPage_UnsupportedKeyAfterSupportedKey_ThrowsNotSupportedException()
    {
        var first = Items.AsCursorQuery()
            .OrderBy(i => i.Id)
            .ThenBy(i => i.Grade)
            .ToCursorPage(pageSize: 5);

        Assert.Throws<NotSupportedException>(() =>
            Items.AsCursorQuery()
                .OrderBy(i => i.Id)
                .ThenBy(i => i.Grade)
                .After(first.EndCursor)
                .ToCursorPage(pageSize: 5));
    }
}
