using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit.Abstractions;
using Zift.Pagination.Cursor;

namespace Zift.Pagination.Tests.Sqlite;

public sealed class CursorParameterizationTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly Commands _commands = new();
    private readonly TestDbContext _db;
    private readonly ITestOutputHelper _output;
    private int _compilations;

    public CursorParameterizationTests(ITestOutputHelper output)
    {
        _output = output;
        _connection.Open();
        _db = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .EnableServiceProviderCaching(false)
            .AddInterceptors(_commands)
            .LogTo(_ => _compilations++, (eventId, _) => eventId == CoreEventId.QueryCompilationStarting)
            .Options);
        _db.Database.EnsureCreated();
        var categories = Enumerable.Range(0, 4).Select(g => new Category
        {
            Title = "category-" + g,
            Rank = g - 1,
            IsActive = g % 2 == 0
        }).ToArray();
        _db.Products.AddRange(Enumerable.Range(1, 8).Select(i => new Product
        {
            Category = categories[(i - 1) / 2],
            Id = i,
            Name = "boundary-" + (i - 1) / 2,
            Score = (i - 1) / 2,
            Key = new Guid((i - 1) / 2 + 1, 0, 0, new byte[8]),
            Flag = i > 4,
            Status = (Status)((i - 1) / 3)
        }));
        _db.SaveChanges();
        _db.ChangeTracker.Clear();
        _commands.Executed.Clear();
    }

    [Theory]
    [InlineData("Name, Id")]
    [InlineData("Score, Id")]
    [InlineData("Key, Id")]
    [InlineData("Flag, Id")]
    [InlineData("Status, Id")]
    public async Task DistinctNonNullCursors_ReuseQueryCompilation_InEachDirection(string ordering)
    {
        await AssertCompilationReuseAsync(_db.Products.AsNoTracking().AsCursorQuery().OrderBy(ordering), ordering);
    }

    [Fact]
    public async Task LiftedTypedIntegerKey_ReusesQueryCompilation_InEachDirection()
    {
        await AssertCompilationReuseAsync(
            _db.Products.AsNoTracking().AsCursorQuery().OrderBy(p => p.Category!.Rank).ThenBy(p => p.Id),
            "typed Category.Rank, Id");
    }

    [Fact]
    public async Task LiftedTypedBooleanKey_ReusesQueryCompilation_InEachDirection()
    {
        await AssertCompilationReuseAsync(
            _db.Products.AsNoTracking().AsCursorQuery().OrderBy(p => p.Category!.IsActive).ThenBy(p => p.Id),
            "typed Category.IsActive, Id");
    }

    private static CursorQueryOptions NullsHigher => new() { NullOrdering = NullOrdering.NullsCompareHigher };

    [Theory]
    [InlineData("Name, Id")]
    [InlineData("Score, Id")]
    [InlineData("Key, Id")]
    [InlineData("Flag, Id")]
    [InlineData("Status, Id")]
    public async Task NullsCompareHigher_DistinctNonNullCursors_ReuseQueryCompilation_InEachDirection(string ordering)
    {
        await AssertCompilationReuseAsync(
            _db.Products.AsNoTracking().AsCursorQuery(NullsHigher).OrderBy(ordering), "NullsCompareHigher " + ordering);
    }

    [Fact]
    public async Task NullsCompareHigher_LiftedTypedIntegerKey_ReusesQueryCompilation_InEachDirection()
    {
        await AssertCompilationReuseAsync(
            _db.Products.AsNoTracking().AsCursorQuery(NullsHigher).OrderBy(p => p.Category!.Rank).ThenBy(p => p.Id),
            "NullsCompareHigher typed Category.Rank, Id");
    }

    [Fact]
    public async Task NullsCompareHigher_LiftedTypedBooleanKey_ReusesQueryCompilation_InEachDirection()
    {
        await AssertCompilationReuseAsync(
            _db.Products.AsNoTracking().AsCursorQuery(NullsHigher).OrderBy(p => p.Category!.IsActive).ThenBy(p => p.Id),
            "NullsCompareHigher typed Category.IsActive, Id");
    }

    [Fact]
    public async Task SqliteSql_NullsCompareHigher_PutsTheNullBranchOnTheOtherSide_AndStaysParameterized()
    {
        var lower = _db.Products.AsNoTracking().AsCursorQuery().OrderBy(p => p.Category!.Rank).ThenBy(p => p.Id);
        var higher = _db.Products.AsNoTracking().AsCursorQuery(NullsHigher).OrderBy(p => p.Category!.Rank).ThenBy(p => p.Id);

        var lowerPage = await lower.ToCursorPageAsync(1);
        await lower.After(lowerPage.EndCursor).ToCursorPageAsync(1);
        var lowerSql = _commands.Executed[^1].Sql;
        var higherPage = await higher.ToCursorPageAsync(1);
        await higher.After(higherPage.EndCursor).ToCursorPageAsync(1);
        var higherCommand = _commands.Executed[^1];
        _output.WriteLine(higherCommand.Sql);

        Assert.Equal(OrderByOf(lowerSql), OrderByOf(higherCommand.Sql));
        Assert.DoesNotContain("\"c\".\"Rank\" IS NULL", lowerSql);
        Assert.Contains("\"c\".\"Rank\" IS NULL OR \"c\".\"Rank\" > @", higherCommand.Sql);
        Assert.DoesNotContain(higherPage.EndCursor!, higherCommand.Sql);
        foreach (var comparison in new[] { "\"Rank\"\\s*>\\s*", "\"Rank\"\\s*=\\s*", "\"Id\"\\s*>\\s*" })
        {
            var parameter = Regex.Match(higherCommand.Sql, comparison + "(?<name>@\\w+)");
            Assert.True(parameter.Success, higherCommand.Sql);
            Assert.Contains(higherCommand.Parameters, p => p.Name == parameter.Groups["name"].Value);
        }
    }

    private static string OrderByOf(string sql) => sql[sql.IndexOf("ORDER BY", StringComparison.Ordinal)..];

    [Fact]
    public async Task SqliteSql_LiftedNavigationKey_UsesLeftJoin_AndParameterizesCursorOperands()
    {
        var order = _db.Products.AsNoTracking().AsCursorQuery().OrderBy(p => p.Category!.Rank).ThenBy(p => p.Id);
        var page = await order.ToCursorPageAsync(1);
        for (var i = 0; i < 3; i++)
        {
            page = await order.After(page.EndCursor).ToCursorPageAsync(1);
            var command = _commands.Executed[^1];
            _output.WriteLine(command.Sql);
            _output.WriteLine(string.Join(", ", command.Parameters.Select(p => $"{p.Name}={p.Value}")));

            Assert.Contains("LEFT JOIN \"Categories\"", command.Sql);
            Assert.Contains("IS NOT NULL", command.Sql);
            foreach (var comparison in new[] { "\"Rank\"\\s*>\\s*", "\"Rank\"\\s*=\\s*", "\"Id\"\\s*>\\s*" })
            {
                var parameter = Regex.Match(command.Sql, comparison + "(?<name>@\\w+)");
                Assert.True(parameter.Success, command.Sql);
                Assert.Contains(command.Parameters, p => p.Name == parameter.Groups["name"].Value);
            }
        }
    }

    private async Task AssertCompilationReuseAsync(IOrderedCursorQuery<Product> order, string ordering)
    {
        var page = await order.ToCursorPageAsync(1);
        var initial = _compilations;
        page = await order.After(page.EndCursor).ToCursorPageAsync(1);
        var forward = _compilations;
        Assert.Equal(initial + 1, forward);
        var cursors = new HashSet<string?> { page.EndCursor };
        for (var i = 0; i < 6; i++)
        {
            page = await order.After(page.EndCursor).ToCursorPageAsync(1);
            cursors.Add(page.EndCursor);
            Assert.Equal(forward, _compilations);
        }
        Assert.Equal(7, cursors.Count);

        page = await order.Before(page.StartCursor).ToCursorPageAsync(1);
        var backward = _compilations;
        Assert.Equal(forward + 1, backward);
        for (var i = 0; i < 6; i++)
        {
            page = await order.Before(page.StartCursor).ToCursorPageAsync(1);
            Assert.Equal(backward, _compilations);
        }
        _output.WriteLine($"EF {typeof(DbContext).Assembly.GetName().Version}; {ordering}: initial={initial}, first After={forward}, six distinct After cursors={forward}, first Before={backward}, six further Before cursors={_compilations}");
    }

    [Fact]
    public async Task SqliteSql_ParameterizesOrderingAndPrefixEqualityOperands()
    {
        var order = _db.Products.AsNoTracking().AsCursorQuery().OrderBy(p => p.Name).ThenBy(p => p.Id);
        var page = await order.ToCursorPageAsync(1);
        for (var i = 0; i < 3; i++)
        {
            page = await order.After(page.EndCursor).ToCursorPageAsync(1);
            var command = _commands.Executed[^1];
            Assert.DoesNotContain("'boundary-", command.Sql);
            foreach (var comparison in new[] { "\"Name\"\\s*>\\s*", "\"Name\"\\s*=\\s*", "\"Id\"\\s*>\\s*" })
            {
                var parameter = Regex.Match(command.Sql, comparison + "(?<name>@\\w+)");
                Assert.True(parameter.Success, command.Sql);
                Assert.Contains(command.Parameters, p => p.Name == parameter.Groups["name"].Value);
            }
            _output.WriteLine(command.Sql);
            _output.WriteLine(string.Join(", ", command.Parameters.Select(p => $"{p.Name}={p.Value}")));
        }
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private sealed record Command(string Sql, (string Name, object? Value)[] Parameters);

    private sealed class Commands : DbCommandInterceptor
    {
        public List<Command> Executed { get; } = [];

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Executed.Add(new Command(command.CommandText,
                command.Parameters.Cast<DbParameter>().Select(p => (p.ParameterName, p.Value)).ToArray()));
            return new(result);
        }
    }
}
