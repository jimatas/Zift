using Zift.Pagination.Cursor;
using Microsoft.EntityFrameworkCore;

namespace Zift.Pagination.Tests.Sqlite;

public sealed class AsyncExecutionTests : IAsyncDisposable
{
    private readonly SqliteFixture _fixture = new();
    public ValueTask DisposeAsync() => _fixture.DisposeAsync();

    private IQueryable<Product> Products => _fixture.Context.Products.AsNoTracking();

    private IOrderedCursorQuery<Product> ById => Products.AsCursorQuery().OrderBy(p => p.Id);

    private sealed class ForeignQuery : IExecutableCursorQuery<Product>;

    private static TException ThrowsEagerly<TException>(Func<Task> call) where TException : Exception
    {
        return Assert.Throws<TException>(() => { _ = call(); });
    }

    [Fact]
    public void ToPageAsync_NullSource_ThrowsEagerly()
    {
        ThrowsEagerly<ArgumentNullException>(() => ((IQueryable<Product>)null!).ToPageAsync(1, 5));
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(-1, 5)]
    [InlineData(1, 0)]
    [InlineData(int.MaxValue, 100)]
    public void ToPageAsync_InvalidPageArguments_ThrowEagerly(int pageNumber, int pageSize)
    {
        ThrowsEagerly<ArgumentOutOfRangeException>(() => Products.OrderBy(p => p.Id).ToPageAsync(pageNumber, pageSize));
    }

    [Fact]
    public void ToCursorPageAsync_NullQuery_ThrowsEagerly()
    {
        ThrowsEagerly<ArgumentNullException>(() => ((IExecutableCursorQuery<Product>)null!).ToCursorPageAsync(5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void ToCursorPageAsync_InvalidPageSize_ThrowsEagerly(int pageSize)
    {
        ThrowsEagerly<ArgumentOutOfRangeException>(() => ById.ToCursorPageAsync(pageSize));
    }

    [Fact]
    public void ToCursorPageAsync_MalformedCursor_ThrowsEagerly()
    {
        ThrowsEagerly<InvalidCursorException>(() => ById.After("garbage").ToCursorPageAsync(5));
    }

    [Fact]
    public async Task ToCursorPageAsync_CursorFromAnotherOrdering_ThrowsEagerly()
    {
        var cursor = (await ById.ToCursorPageAsync(2)).EndCursor;
        var byName = Products.AsCursorQuery().OrderBy(p => p.Name).ThenBy(p => p.Id);

        ThrowsEagerly<InvalidCursorException>(() => byName.After(cursor).ToCursorPageAsync(5));
    }

    [Fact]
    public void ToCursorPageAsync_WithoutOrdering_ThrowsEagerly()
    {
        var unordered = (IExecutableCursorQuery<Product>)Products.AsCursorQuery();

        ThrowsEagerly<InvalidOperationException>(() => unordered.ToCursorPageAsync(5));
    }

    [Fact]
    public void ToCursorPageAsync_QueryNotCreatedByAsCursorQuery_ThrowsEagerly()
    {
        ThrowsEagerly<InvalidOperationException>(() => new ForeignQuery().ToCursorPageAsync(5));
    }

    [Fact]
    public async Task ToPageAsync_CancelledToken_FaultsTheTask_InsteadOfThrowing()
    {
        var task = Products.OrderBy(p => p.Id).ToPageAsync(1, 5, new CancellationToken(canceled: true));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
    }

    [Fact]
    public async Task ToCursorPageAsync_CancelledToken_FaultsTheTask_InsteadOfThrowing()
    {
        var task = ById.ToCursorPageAsync(5, new CancellationToken(canceled: true));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
    }

    [Fact]
    public async Task ToPageAsync_InMemorySource_FaultsTheTask_WithInvalidOperationException()
    {
        var task = Enumerable.Range(1, 5).AsQueryable().ToPageAsync(1, 2);

        await Assert.ThrowsAsync<InvalidOperationException>(() => task);
    }

    [Fact]
    public async Task ToCursorPageAsync_InMemorySource_FaultsTheTask_WithInvalidOperationException()
    {
        var task = Enumerable.Range(1, 5).AsQueryable().AsCursorQuery().OrderBy(i => i).ToCursorPageAsync(2);

        await Assert.ThrowsAsync<InvalidOperationException>(() => task);
    }
}
