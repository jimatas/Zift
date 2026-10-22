using Zift.Pagination.Cursor;

namespace Zift.Pagination.Tests.Sqlite;

public sealed class CursorQueryValidationTests
{
    private sealed class ForeignQuery : IExecutableCursorQuery<int>;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NullQuery_ThrowsSynchronously_BeforePageSizeValidation(bool isAsync)
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => InvokePagination(null!, 0, isAsync));

        Assert.Equal("query", exception.ParamName);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, -1)]
    [InlineData(false, int.MaxValue)]
    [InlineData(true, 0)]
    [InlineData(true, -1)]
    [InlineData(true, int.MaxValue)]
    public void InvalidPageSize_ThrowsSynchronously_BeforeImplementationValidation(bool isAsync, int pageSize)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => InvokePagination(new ForeignQuery(), pageSize, isAsync));

        Assert.Equal("pageSize", exception.ParamName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForeignQuery_ThrowsSynchronously_WithExistingMessage(bool isAsync)
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => InvokePagination(new ForeignQuery(), 1, isAsync));

        Assert.Equal("The cursor query was not created by AsCursorQuery().", exception.Message);
    }

    private static void InvokePagination(IExecutableCursorQuery<int> query, int pageSize, bool isAsync)
    {
        if (isAsync)
        {
            _ = query.ToCursorPageAsync(pageSize);
        }
        else
        {
            _ = query.ToCursorPage(pageSize);
        }
    }
}
