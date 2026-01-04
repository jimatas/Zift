namespace Zift.Fixture;

public sealed class Review
{
    public Guid Id { get; set; }
    public string? Content { get; set; }
    public int? Rating { get; set; }
    public DateTime? DatePosted { get; set; }
    public User? Author { get; set; }
}
