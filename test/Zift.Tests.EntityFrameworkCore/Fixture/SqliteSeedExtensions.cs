namespace Zift.Fixture;

public static class SqliteSeedExtensions
{
    public static async Task SeedAsync(
        this SqliteTestFixture fixture,
        IReadOnlyCollection<Category> categories)
    {
        fixture.Context.Categories.AddRange(categories);
        await fixture.Context.SaveChangesAsync();
    }
}
