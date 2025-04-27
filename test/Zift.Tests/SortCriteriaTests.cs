namespace Zift.Tests;

using SharedFixture.Models;
using Sorting;

public class SortCriteriaTests
{
    [Fact]
    public void ByDefault_IsEmpty()
    {
        var criteria = new SortCriteria<Product>();

        Assert.Empty(criteria);
    }

    [Fact]
    public void Add_SingleCriterion_AddsCriterion()
    {
        var criteria = new SortCriteria<Product>();
        var criterion = new SortCriterion<Product>("Name", SortDirection.Ascending);

        criteria.Add(criterion);
        
        Assert.Single(criteria);
        Assert.Same(criterion, criteria.First());
    }

    [Fact]
    public void Add_MultipleCriteria_AddsAllCriteria()
    {
        var criteria = new SortCriteria<Product>();
        var criterion1 = new SortCriterion<Product>("Name", SortDirection.Ascending);
        var criterion2 = new SortCriterion<Product>("Price", SortDirection.Descending);

        criteria.Add(criterion1);
        criteria.Add(criterion2);

        Assert.Equal(2, criteria.Count());
        Assert.Same(criterion1, criteria.ElementAt(0));
        Assert.Same(criterion2, criteria.ElementAt(1));
    }

    [Fact]
    public void Add_NullCriterion_ThrowsArgumentNullException()
    {
        var criteria = new SortCriteria<Product>();
        
        Assert.Throws<ArgumentNullException>("criterion", () => criteria.Add(null!));
    }

    [Fact]
    public void ApplyTo_NullQuery_ThrowsArgumentNullException()
    {
        var criteria = new SortCriteria<Product>();

        Assert.Throws<ArgumentNullException>("query", () => criteria.ApplyTo(null!));
    }

    [Fact]
    public void ApplyTo_WithSortCriteria_AppliesSorting()
    {
        var products = new[]
        {
            new Product { Name = "Zebra" },
            new Product { Name = "Apple" },
            new Product { Name = "Banana" }
        }.AsQueryable();

        var criteria = new SortCriteria<Product>
        {
            new SortCriterion<Product>("Name", SortDirection.Ascending)
        };

        var result = criteria.ApplyTo(products).ToList();

        Assert.Equal("Apple", result[0].Name);
        Assert.Equal("Banana", result[1].Name);
        Assert.Equal("Zebra", result[2].Name);
    }

    [Fact]
    public void ApplyTo_WithMultipleSortCriteria_AppliesAllInOrder()
    {
        var products = new[]
        {
            new Product { Name = "Product 2", Price = 10 },
            new Product { Name = "Product 1", Price = 10 },
            new Product { Name = "Product 1", Price = 20 }
        }.AsQueryable();

        var criteria = new SortCriteria<Product>
        {
            new SortCriterion<Product>("Name", SortDirection.Ascending),
            new SortCriterion<Product>("Price", SortDirection.Ascending)
        };

        var result = criteria.ApplyTo(products).ToList();

        var expected = products
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Price)
            .ToList();

        Assert.Equal(expected.Select(p => (p.Name, p.Price)), result.Select(p => (p.Name, p.Price)));
    }

    [Fact]
    public void ApplyTo_WithoutSortCriteria_UsesOriginalOrder()
    {
        var products = new[]
        {
            new Product { Name = "Zebra" },
            new Product { Name = "Apple" },
            new Product { Name = "Banana" }
        }.AsQueryable();

        var criteria = new SortCriteria<Product>();

        var result = criteria.ApplyTo(products).ToList();

        Assert.Equal("Zebra", result[0].Name);
        Assert.Equal("Apple", result[1].Name);
        Assert.Equal("Banana", result[2].Name);
    }

    [Fact]
    public void GetEnumerator_ReturnsExpectedCriteria()
    {
        var criteria = new SortCriteria<Product>
        {
            new SortCriterion<Product>("Name", SortDirection.Ascending),
            new SortCriterion<Product>("Price", SortDirection.Descending)
        };

        IEnumerable enumerable = criteria;
        var enumerator = enumerable.GetEnumerator();

        Assert.True(enumerator.MoveNext());
        Assert.Equal("Name", (enumerator.Current as ISortCriterion)?.Property);

        Assert.True(enumerator.MoveNext());
        Assert.Equal("Price", (enumerator.Current as ISortCriterion)?.Property);
    }
}
