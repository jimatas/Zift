using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Zift.Pagination.Tests.Sqlite;

public sealed class SqliteFixture : IAsyncDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public SqliteFixture(int count = 23)
    {
        _connection.Open();

        Context = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>().UseSqlite(_connection).Options);
        Context.Database.EnsureCreated();

        var books = new Category { Title = "Books", Rank = -3, IsActive = true };
        var games = new Category { Title = "Games", Rank = 0, IsActive = false, Parent = books };
        var toys = new Category { Title = "Toys", Rank = 7, IsActive = true, Parent = games };
        var software = new Category { Title = "Software", Rank = 3, IsActive = false, Parent = books };
        var categories = new[] { books, games, toys };
        Context.Categories.AddRange(categories);
        Context.Categories.Add(software);

        for (var i = 1; i <= count; i++)
        {
            Context.Products.Add(new Product
            {
                Name = $"Name{i % 4}",                              // many ties
                Nick = i % 3 == 0 ? null : $"nick{i % 5}",          // nulls and ties
                Score = i % 4 == 0 ? null : i % 6,                  // nulls and ties
                Status = (Status)(i % 3),
                Flag = i % 2 == 0,
                Key = new Guid(i, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
                Created = new DateTime(2024, 1, 1).AddDays(i % 7),
                Price = i % 5 * 1.5,
                Category = i % 4 == 0 ? null : categories[i % 3],
                AlternateCategory = i % 5 == 0 ? null! : categories[(i + 1) % 3]
            });
        }

        Context.SaveChanges();
        Context.ChangeTracker.Clear();
    }

    public TestDbContext Context { get; }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
