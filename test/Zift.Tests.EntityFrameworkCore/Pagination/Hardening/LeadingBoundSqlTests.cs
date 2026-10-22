using System.Data.Common;
using Zift.Pagination.Cursor;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Zift.Pagination.Tests.Sqlite;

public sealed class LeadingBoundSqlTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly SqliteConnection _optionalNameConnection = new("DataSource=:memory:");
    private readonly Commands _commands = new();
    private readonly TestDbContext _db;
    private readonly OptionalNameContext _optionalName;

    private static CursorQueryOptions NullsHigher => new() { NullOrdering = NullOrdering.NullsCompareHigher };

    public LeadingBoundSqlTests()
    {
        _connection.Open();
        _db = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection).AddInterceptors(_commands).Options);
        _db.Database.EnsureCreated();
        _db.Products.AddRange(Enumerable.Range(1, 6).Select(i => new Product
        {
            Id = i, Name = "n" + i / 2, Nick = "k" + i / 2, Score = i / 2, Flag = i > 3
        }));
        _db.SaveChanges();

        _optionalNameConnection.Open();
        _optionalName = new OptionalNameContext(new DbContextOptionsBuilder<OptionalNameContext>()
            .UseSqlite(_optionalNameConnection).AddInterceptors(_commands).Options);
        _optionalName.Database.EnsureCreated();
        _optionalName.Products.AddRange(Enumerable.Range(1, 6).Select(i => new Product { Id = i, Name = "n" + i / 2 }));
        _optionalName.SaveChanges();
    }

    [Fact]
    public async Task RequiredString_UnderNullsHigher_GetsAPlainRange_WithNoNullGuard()
    {
        var sql = await SqlAfterAsync(_db.Products.AsNoTracking().AsCursorQuery(NullsHigher).OrderBy(p => p.Name).ThenBy(p => p.Id));

        Assert.Contains("\"p\".\"Name\" >= @", sql);
        Assert.DoesNotContain("\"p\".\"Name\" IS NULL", sql);
    }

    [Fact]
    public async Task OptionalString_UnderNullsHigher_KeepsTheNullGuard_InFrontOfTheBound()
    {
        var sql = await SqlAfterAsync(_db.Products.AsNoTracking().AsCursorQuery(NullsHigher).OrderBy(p => p.Nick).ThenBy(p => p.Id));

        Assert.Contains("(\"p\".\"Nick\" IS NULL OR \"p\".\"Nick\" >= @", sql);
    }

    [Fact]
    public async Task OptionalString_DescendingUnderNullsLower_KeepsTheNullGuard_BecauseNullsSortLast()
    {
        var sql = await SqlAfterAsync(_db.Products.AsNoTracking().AsCursorQuery().OrderByDescending(p => p.Nick).ThenBy(p => p.Id));

        Assert.Contains("(\"p\".\"Nick\" IS NULL OR \"p\".\"Nick\" <= @", sql);
    }

    [Fact]
    public async Task OptionalInt_AscendingUnderNullsLower_NeedsNoOrBranch_BecauseNullsSortFirst()
    {
        var sql = await SqlAfterAsync(_db.Products.AsNoTracking().AsCursorQuery().OrderBy(p => p.Score).ThenBy(p => p.Id));

        Assert.Contains("\"p\".\"Score\" >= @", sql);
        Assert.DoesNotContain("\"p\".\"Score\" IS NULL", sql);
    }

    [Fact]
    public async Task NonNullableStringProperty_ConfiguredOptionalInTheModel_KeepsTheNullGuard()
    {
        var order = _optionalName.Products.AsNoTracking().AsCursorQuery(NullsHigher).OrderBy(p => p.Name).ThenBy(p => p.Id);

        var sql = await SqlAfterAsync(order);

        Assert.Contains("(\"p\".\"Name\" IS NULL OR \"p\".\"Name\" >= @", sql);
    }

    [Fact]
    public async Task TwoKeys_BoundIsParameterized_AndFollowsTheDirectionOnBefore()
    {
        var order = _db.Products.AsNoTracking().AsCursorQuery().OrderBy(p => p.Name).ThenBy(p => p.Id);
        var page = await order.ToCursorPageAsync(2);

        await order.After(page.EndCursor).ToCursorPageAsync(2);
        var after = _commands.Executed[^1];
        await order.Before(page.EndCursor).ToCursorPageAsync(2);
        var before = _commands.Executed[^1];

        Assert.Contains("\"p\".\"Name\" >= @", after);
        Assert.Contains("\"p\".\"Name\" <= @", before);
        Assert.DoesNotContain("'n", after);
        Assert.DoesNotContain(page.EndCursor!, after);
    }

    [Theory]
    [InlineData("Id")]
    [InlineData("Flag, Id")]
    public async Task SingleKey_AndBooleanLeadingKey_GetNoBound(string ordering)
    {
        var sql = await SqlAfterAsync(_db.Products.AsNoTracking().AsCursorQuery().OrderBy(ordering));

        Assert.DoesNotContain(">=", sql);
        Assert.DoesNotContain("<=", sql);
    }

    private async Task<string> SqlAfterAsync(IOrderedCursorQuery<Product> order)
    {
        var page = await order.ToCursorPageAsync(2);
        await order.After(page.EndCursor).ToCursorPageAsync(2);

        return _commands.Executed[^1];
    }

    public void Dispose()
    {
        _db.Dispose();
        _optionalName.Dispose();
        _connection.Dispose();
        _optionalNameConnection.Dispose();
    }

    private sealed class OptionalNameContext(DbContextOptions<OptionalNameContext> options) : DbContext(options)
    {
        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().Property(p => p.Name).IsRequired(false);
            modelBuilder.Entity<Product>().Ignore(p => p.Category).Ignore(p => p.AlternateCategory);
        }
    }

    private sealed class Commands : DbCommandInterceptor
    {
        public List<string> Executed { get; } = [];

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Executed.Add(command.CommandText);
            return new(result);
        }
    }
}
