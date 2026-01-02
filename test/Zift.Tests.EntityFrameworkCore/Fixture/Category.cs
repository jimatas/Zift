namespace Zift.Fixture;

public sealed class Category
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public ICollection<Product> Products { get; set; } = [];
}
