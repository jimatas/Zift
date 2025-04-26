namespace Zift.Tests;

using Filtering.Dynamic;
using Fixture;

public class PropertyPathTests
{
    [Fact]
    public void Constructor_NullSegments_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("segments", () => _ = new PropertyPath(null!));
    }

    [Fact]
    public void Constructor_EmptySegments_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>("segments", () => _ = new PropertyPath([]));
    }

    [Theory]
    [InlineData("Name", 1)]
    [InlineData("Products.Name", 2)]
    [InlineData("Products:any.Name", 2)]
    [InlineData("Products:Any.Name", 2, "Products:any.Name")]
    [InlineData("Products:all.Name", 2)]
    [InlineData("Products:All.Name", 2, "Products:all.Name")]
    [InlineData("Products.Reviews.Author", 3)]
    [InlineData("Products.Reviews:any.Rating", 3)]
    [InlineData("Products.Reviews.Rating", 3)]
    [InlineData("Products.Reviews:count", 2)]
    [InlineData("Products:count", 1)]
    [InlineData("Products:all.Reviews.Rating", 3)]
    [InlineData("Products.Reviews:all.Author", 3)]
    [InlineData("Products.Reviews.Author.Name", 4)]
    [InlineData("Products:all.Reviews:count", 2)]
    public void Constructor_ValidPathSegments_ParsesAndNormalizesCorrectly(string rawPropertyPath, int expectedSegmentCount, string? normalizedPropertyPath = null)
    {
        var propertyPath = PropertyPathFactory.Create(rawPropertyPath);

        Assert.Equal(expectedSegmentCount, propertyPath.Count);
        Assert.Equal(normalizedPropertyPath ?? rawPropertyPath, propertyPath.ToString(includeModifiers: true));
    }

    [Fact]
    public void Constructor_InvalidModifierCombination_ThrowsArgumentException()
    {
        var segments = new[]
        {
            new PropertyPathSegment("Products")
            {
                Quantifier = QuantifierMode.Any,
                Projection = CollectionProjection.Count
            }
        };

        var ex = Assert.Throws<ArgumentException>(nameof(segments), () => _ = new PropertyPath(segments));
        Assert.StartsWith("A property segment cannot have more than one modifier", ex.Message);
    }

    [Fact]
    public void Constructor_TerminalSegmentNotAtEnd_ThrowsArgumentException()
    {
        var segments = new[]
        {
            new PropertyPathSegment("Products") { Projection = CollectionProjection.Count },
            new PropertyPathSegment("Reviews")
        };

        var ex = Assert.Throws<ArgumentException>(nameof(segments), () => _ = new PropertyPath(segments));
        Assert.StartsWith("Terminal collection projections (e.g., ':count') must appear at the end of the property path.", ex.Message);
    }

    [Fact]
    public void ToString_WithoutModifiers_ReturnsStringWithoutModifiers()
    {
        var result = PropertyPathFactory.Create("Products:all.Name").ToString();

        Assert.Equal("Products.Name", result);
    }

    [Fact]
    public void ToString_WithModifiers_ReturnsStringWithModifiers()
    {
        var rawPropertyPath = "Products:all.Reviews:count";
        var result = PropertyPathFactory.Create(rawPropertyPath).ToString(includeModifiers: true);

        Assert.Equal(rawPropertyPath, result);
    }
}
