namespace Zift.Pagination.Cursor;

using Fixture;

public sealed class CursorQueryExtensionsTests
{
    [Fact]
    public void ToCursorPage_NullQuery_ThrowsArgumentNullException()
    {
        IExecutableCursorQuery<TestClass> query = null!;

        Assert.Throws<ArgumentNullException>(() => query.ToCursorPage(pageSize: 1));
    }

    [Fact]
    public void ToCursorPage_PageSizeLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        var source = Array.Empty<TestClass>().AsQueryable();

        var query = source
            .AsCursorQuery()
            .OrderBy(e => e.Int32Value);

        Assert.Throws<ArgumentOutOfRangeException>(() => query.ToCursorPage(pageSize: 0));
    }

    [Fact]
    public void ToCursorPage_QueryWithoutExecutionState_ThrowsInvalidOperationException()
    {
        var query = new FakeExecutableCursorQuery<TestClass>();

        var ex = Assert.Throws<InvalidOperationException>(() => query.ToCursorPage(pageSize: 1));

        Assert.Contains("execution state", ex.Message);
    }

    private sealed class FakeExecutableCursorQuery<T>
        : IExecutableCursorQuery<T>;
}
