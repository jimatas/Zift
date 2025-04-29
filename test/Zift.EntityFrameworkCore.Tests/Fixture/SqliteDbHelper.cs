namespace Zift.EntityFrameworkCore.Tests.Fixture;

using Microsoft.EntityFrameworkCore;

public static class SqliteDbHelper
{
    public static async Task<CatalogDbContext> CreateDatabaseAsync(Action<CatalogDbContext>? seeder = null)
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var dbContext = new CatalogDbContext(options);
        
        await dbContext.Database.OpenConnectionAsync();
        await dbContext.Database.EnsureCreatedAsync();

        if (seeder is not null)
        {
            seeder(dbContext);
            await dbContext.SaveChangesAsync();
        }

        return dbContext;
    }
}
