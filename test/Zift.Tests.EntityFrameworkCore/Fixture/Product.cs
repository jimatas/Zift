namespace Zift.Fixture;

public sealed class Product
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public ICollection<Review> Reviews { get; set; } = [];
}
