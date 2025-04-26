namespace Zift.Tests;

using Filtering.Dynamic;

public class PropertyPathSegmentTests
{
    [Fact]
    public void ToString_WithoutModifier_ReturnsSegmentName()
    {
        var result = new PropertyPathSegment("Products")
        {
            Quantifier = QuantifierMode.Any
        }.ToString();

        Assert.Equal("Products", result);
    }

    [Fact]
    public void ToString_WithModifier_ReturnsSegmentNameWithModifier()
    {
        var result = new PropertyPathSegment("Products")
        {
            Quantifier = QuantifierMode.Any
        }.ToString(includeModifier: true);

        Assert.Equal("Products:any", result);
    }
}
