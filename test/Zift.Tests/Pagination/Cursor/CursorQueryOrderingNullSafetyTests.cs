namespace Zift.Pagination.Cursor;

using Ordering;

public sealed class CursorQueryOrderingNullSafetyTests
{
    private readonly OrderingOptions _withNullGuards = new() { EnableNullGuards = true };

    [Fact]
    public void OrderBy_NestedPath_NullAtFirstLevel_DoesNotThrow()
    {
        var source = new[]
        {
            new Root { L1 = null },
            new Root { L1 = new Level1 { L2 = new Level2 { Value = 2 } } },
            new Root { L1 = new Level1 { L2 = new Level2 { Value = 1 } } }
        }.AsQueryable();

        var ex = Record.Exception(() =>
            source.AsCursorQuery()
                .OrderBy("L1.L2.Value", _withNullGuards)
                .ToCursorPage(pageSize: 10));

        Assert.Null(ex);
    }

    [Fact]
    public void OrderBy_NestedPath_NullAtIntermediateLevel_DoesNotThrow()
    {
        var source = new[]
        {
            new Root { L1 = new Level1 { L2 = null } },
            new Root { L1 = new Level1 { L2 = new Level2 { Value = 2 } } },
            new Root { L1 = new Level1 { L2 = new Level2 { Value = 1 } } }
        }.AsQueryable();

        var ex = Record.Exception(() =>
            source.AsCursorQuery()
                .OrderBy("L1.L2.Value", _withNullGuards)
                .ToCursorPage(pageSize: 10));

        Assert.Null(ex);
    }

    [Fact]
    public void OrderBy_NestedPath_OrdersCorrectly()
    {
        var source = new[]
        {
            new Root { L1 = null },
            new Root { L1 = new Level1 { L2 = null } },
            new Root { L1 = new Level1 { L2 = new Level2 { Value = 2 } } },
            new Root { L1 = new Level1 { L2 = new Level2 { Value = 1 } } }
        }.AsQueryable();

        var ordered = source
            .AsCursorQuery()
            .OrderBy("L1.L2.Value", _withNullGuards)
            .ToCursorPage(pageSize: 10);

        Assert.Equal(
            [null, null, 1, 2],
            ordered.Items.Select(x => x.L1?.L2?.Value));
    }

    [Fact]
    public void OrderByDescending_NestedPath_OrdersCorrectly()
    {
        var source = new[]
        {
            new Root { L1 = null },
            new Root { L1 = new Level1 { L2 = new Level2 { Value = 1 } } },
            new Root { L1 = new Level1 { L2 = new Level2 { Value = 2 } } }
        }.AsQueryable();

        var ordered = source
            .AsCursorQuery()
            .OrderBy("L1.L2.Value DESC", _withNullGuards)
            .ToCursorPage(pageSize: 10);

        Assert.Equal(
            [2, 1, null],
            ordered.Items.Select(x => x.L1?.L2?.Value));
    }

    private sealed class Root
    {
        public Level1? L1 { get; set; }
    }

    private sealed class Level1
    {
        public Level2? L2 { get; set; }
    }

    private sealed class Level2
    {
        public int Value { get; set; }
    }
}
