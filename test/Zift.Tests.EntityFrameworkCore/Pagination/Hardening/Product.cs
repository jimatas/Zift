namespace Zift.Pagination.Tests.Sqlite;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Nick { get; set; }
    public int? Score { get; set; }
    public Status Status { get; set; }
    public bool Flag { get; set; }
    public Guid Key { get; set; }
    public DateTime Created { get; set; }
    public double Price { get; set; }
    public Category? Category { get; set; }
    public int? CategoryId { get; set; }

    public Category AlternateCategory { get; set; } = null!;
    public int? AlternateCategoryId { get; set; }
}
