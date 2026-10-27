using Microsoft.EntityFrameworkCore;

namespace Zift.Pagination.Hardening;

public sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();
}
