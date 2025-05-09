namespace Zift.Tests;

using Filtering;
using SharedFixture.Models;

public class DynamicFilterCriteriaNullSafetyTests
{
    [Fact]
    public void Filter_ByNestedNullProperty_DoesNotThrowAndReturnsExpected()
    {
        var products = new[]
        {
            new Product
            {
                Name = "Tablet",
                Reviews =
                {
                    new Review { Author = null, Rating = 5 },
                    new Review { Author = new User { Name = null, Email = "anonymous@example.com" }, Rating = 4 }
                }
            },
            new Product
            {
                Name = "Headphones",
                Reviews =
                {
                    new Review { Author = new User { Name = "John Doe", Email = "john.doe@example.com" }, Rating = 5 }
                }
            }
        };

        var filter = new DynamicFilterCriteria<Product>("Reviews.Author.Name == 'John Doe'");

        var result = products.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Headphones", result[0].Name);
    }

    [Fact]
    public void Filter_DeeplyNestedNullChain_DoesNotThrowAndReturnsEmpty()
    {
        var products = new[]
        {
            new Product
            {
                Name = "Camera",
                Reviews = null! // Entire collection is null
            }
        };

        var filter = new DynamicFilterCriteria<Product>("Reviews.Author.Name == 'Anyone'");

        var result = products.AsQueryable().Filter(filter).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Filter_NullableScalarProperty_HandledSafely()
    {
        var reviews = new[]
        {
            new Review { Rating = null },
            new Review { Rating = 5 }
        };

        var filter = new DynamicFilterCriteria<Review>("Rating >= 4");

        var result = reviews.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal(5, result[0].Rating);
    }

    [Fact]
    public void Filter_ByNullEqualityCheck_MatchesOnlyNulls()
    {
        var reviews = new[]
        {
            new Review { Rating = null },
            new Review { Rating = 5 }
        };

        var filter = new DynamicFilterCriteria<Review>("Rating == null");

        var result = reviews.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Null(result[0].Rating);
    }

    [Fact]
    public void Filter_ByNonNullCheck_MatchesOnlyNonNulls()
    {
        var reviews = new[]
        {
            new Review { Rating = null },
            new Review { Rating = 4 }
        };

        var filter = new DynamicFilterCriteria<Review>("Rating != null");

        var result = reviews.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal(4, result[0].Rating);
    }

    [Fact]
    public void Filter_StartsWithOperatorWhenStringIsNull_DoesNotThrowAndSkips()
    {
        var products = new[]
        {
            new Product { Name = null },
            new Product { Name = "Smartphone" }
        };

        var filter = new DynamicFilterCriteria<Product>("Name ^= 'S'");

        var result = products.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Smartphone", result[0].Name);
    }

    [Fact]
    public void Filter_EndsWithOperatorWhenStringIsNull_DoesNotThrowAndSkips()
    {
        var products = new[]
        {
            new Product { Name = null },
            new Product { Name = "Smartphone" }
        };

        var filter = new DynamicFilterCriteria<Product>("Name $= 'phone'");

        var result = products.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Smartphone", result[0].Name);
    }

    [Fact]
    public void Filter_EqualityOperatorWithNullComparisonValue_MatchesNull()
    {
        var products = new[]
        {
            new Product { Name = null },
            new Product { Name = "Laptop" }
        };

        var filter = new DynamicFilterCriteria<Product>("Name == null");

        var result = products.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Null(result[0].Name);
    }

    [Fact]
    public void Filter_StartsWithOperatorWithNullComparisonValue_DoesNotThrowAndSkips()
    {
        var products = new[]
        {
            new Product { Name = "Smartphone" },
            new Product { Name = "Tablet" }
        };

        var filter = new DynamicFilterCriteria<Product>("Name ^= null");

        var result = products.AsQueryable().Filter(filter).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Filter_ByCollectionQuantifierOnNullCollection_DoesNotThrowAndReturnsEmpty()
    {
        var categories = new[]
        {
            new Category
            {
                Name = "Empty",
                Products = null! // entire collection is null
            }
        };

        var filter = new DynamicFilterCriteria<Category>("Products:any.Price > 10");

        var result = categories.AsQueryable().Filter(filter).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Filter_ExplicitAllWithEmptyCollection_EvaluatesCorrectly()
    {
        var categories = new[]
        {
            new Category
            {
                Name = "NoProducts",
                Products = [] // empty list
            }
        };

        var filter = new DynamicFilterCriteria<Category>("Products:all.Price > 0");

        var result = categories.AsQueryable().Filter(filter).ToList();

        Assert.Single(result); // If vacuous truth: all() is true on empty
        Assert.Equal("NoProducts", result[0].Name);
    }

    [Fact]
    public void Filter_AnyWithNullItemInCollection_DoesNotThrow()
    {
        var categories = new[]
        {
            new Category
            {
                Name = "Mixed",
                Products = [null!, new Product { Price = 999 }]
            }
        };

        var filter = new DynamicFilterCriteria<Category>("Products:any.Price > 500");

        var result = categories.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Mixed", result[0].Name);
    }

    [Fact]
    public void Filter_ByProductNameInList_DoesNotThrowOnNulls()
    {
        var products = new[]
        {
            new Product { Name = null },
            new Product { Name = "Tablet" }
        };

        var filter = new DynamicFilterCriteria<Product>("Name in ['Tablet', 'Phone']");

        var result = products.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Tablet", result[0].Name);
    }

    [Fact]
    public void Filter_ByProductNameInListIgnoreCase_SkipsNullsAndMatches()
    {
        var products = new[]
        {
            new Product { Name = null },
            new Product { Name = "Tablet" }
        };

        var filter = new DynamicFilterCriteria<Product>("Name in:i ['TABLET', null]");

        var result = products.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Tablet", result[0].Name);
    }
}
