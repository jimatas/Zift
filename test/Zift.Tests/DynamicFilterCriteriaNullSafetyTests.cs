﻿namespace Zift.Tests;

using Filtering;
using Filtering.Dynamic;
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

        var options = new FilterOptions { EnableNullGuards = true };
        var filter = new DynamicFilterCriteria<Product>("Reviews.Author.Name == 'John Doe'", options);

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

        var options = new FilterOptions { EnableNullGuards = true };
        var filter = new DynamicFilterCriteria<Product>("Reviews.Author.Name == 'Anyone'", options);

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

        var options = new FilterOptions { EnableNullGuards = true };
        var filter = new DynamicFilterCriteria<Review>("Rating >= 4", options);

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

        var options = new FilterOptions { EnableNullGuards = true };
        var filter = new DynamicFilterCriteria<Review>("Rating == null", options);

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

        var options = new FilterOptions { EnableNullGuards = true };
        var filter = new DynamicFilterCriteria<Review>("Rating != null", options);

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

        var options = new FilterOptions { EnableNullGuards = true };
        var filter = new DynamicFilterCriteria<Product>("Name ^= 'S'", options);

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

        var options = new FilterOptions { EnableNullGuards = true };
        var filter = new DynamicFilterCriteria<Product>("Name $= 'phone'", options);

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

        var options = new FilterOptions { EnableNullGuards = true };
        var filter = new DynamicFilterCriteria<Product>("Name == null", options);

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

        var options = new FilterOptions { EnableNullGuards = true };
        var filter = new DynamicFilterCriteria<Product>("Name ^= null", options);

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

        var options = new FilterOptions { EnableNullGuards = true };
        var filter = new DynamicFilterCriteria<Category>("Products:any.Price > 10", options);

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

        var options = new FilterOptions { EnableNullGuards = true };
        var filter = new DynamicFilterCriteria<Category>("Products:all.Price > 0", options);

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

        var options = new FilterOptions { EnableNullGuards = true };
        var filter = new DynamicFilterCriteria<Category>("Products:any.Price > 500", options);

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

        var options = new FilterOptions { EnableNullGuards = true };
        var filter = new DynamicFilterCriteria<Product>("Name in ['Tablet', 'Phone']", options);

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

        var options = new FilterOptions { EnableNullGuards = true };
        var filter = new DynamicFilterCriteria<Product>("Name in:i ['TABLET', null]", options);

        var result = products.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Tablet", result[0].Name);
    }

    [Fact]
    public void Filter_ByCollectionProjectionWithNullCollection_DoesNotThrowAndReturnsEmpty()
    {
        var categories = new[]
        {
            new Category
            {
                Name = "Test",
                Products = null!
            }
        };

        var options = new FilterOptions { EnableNullGuards = true };
        var filter = new DynamicFilterCriteria<Category>("Products:count == 0", options);

        var result = categories.AsQueryable().Filter(filter).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Filter_ByValueTypeOperandWithInOperator_FiltersCorrectly()
    {
        var products = new[]
        {
            new Product { Name = "Smartphone", Price = 100 },
            new Product { Name = "Tablet", Price = 200 }
        };

        var options = new FilterOptions { EnableNullGuards = true };
        var filter = new DynamicFilterCriteria<Product>("Price in [100, 300]", options);

        var result = products.AsQueryable().Filter(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Smartphone", result[0].Name);
    }
}
