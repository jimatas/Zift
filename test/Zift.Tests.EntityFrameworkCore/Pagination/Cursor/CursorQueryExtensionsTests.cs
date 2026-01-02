namespace Zift.Pagination.Cursor;

using Fixture;

public sealed class CursorQueryExtensionsTests
{
    [Fact]
    public async Task ToCursorPageAsync_NullQuery_ThrowsArgumentNullException()
    {
        IExecutableCursorQuery<Category> query = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            query.ToCursorPageAsync(pageSize: 1));
    }

    [Fact]
    public async Task ToCursorPageAsync_PageSizeLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        var source = Array.Empty<Category>().AsQueryable();

        var query = source
            .AsCursorQuery()
            .OrderBy(c => c.Name);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            query.ToCursorPageAsync(pageSize: 0));
    }

    [Fact]
    public async Task ToCursorPageAsync_QueryWithoutExecutionState_ThrowsInvalidOperationException()
    {
        IExecutableCursorQuery<Category> query =
            new FakeExecutableCursorQuery<Category>();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            query.ToCursorPageAsync(pageSize: 1));

        Assert.Contains("execution state", ex.Message);
    }

    private sealed class FakeExecutableCursorQuery<T>
        : IExecutableCursorQuery<T>;
}
