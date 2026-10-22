namespace Zift.Pagination.Tests.Sqlite;

public class Category
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int Rank { get; set; }
    public bool IsActive { get; set; }
    public Category? Parent { get; set; }
    public int? ParentId { get; set; }
    public ICollection<Category> Children { get; set; } = [];
}
