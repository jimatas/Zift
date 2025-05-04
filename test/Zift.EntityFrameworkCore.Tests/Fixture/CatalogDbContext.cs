namespace Zift.EntityFrameworkCore.Tests.Fixture;

using Microsoft.EntityFrameworkCore;
using SharedFixture.Models;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Category>(builder =>
        {
            builder.ToTable("Categories");
            builder.HasKey(category => category.Id);
            builder.Property(category => category.Name).HasMaxLength(100);
            builder.HasMany(category => category.Products)
                .WithOne()
                .HasForeignKey("CategoryId");
        });

        model.Entity<Product>(builder =>
        {
            builder.ToTable("Products");
            builder.HasKey(product => product.Id);
            builder.Property("CategoryId");
            builder.Property(product => product.Name).HasMaxLength(100);
            builder.Property(product => product.Price);
            builder.Property(product => product.Description).HasMaxLength(500);
            builder.HasMany(product => product.Reviews)
                .WithOne()
                .HasForeignKey("ProductId");
        });

        model.Entity<Review>(builder =>
        {
            builder.ToTable("Reviews");
            builder.HasKey(review => review.Id);
            builder.Property(review => review.Rating);
            builder.Property(review => review.Content).HasMaxLength(500);
            builder.Property(review => review.DatePosted);
            builder.OwnsOne(b => b.Author, author =>
            {
                author.WithOwner();
                author.Property(a => a.Name).HasColumnName("Author").HasMaxLength(100);
                author.Property(a => a.Email).HasColumnName("AuthorEmail").HasMaxLength(100);
            });
        });

        base.OnModelCreating(model);
    }
}
