namespace Zift.Tests;

using Filtering.Dynamic;
using Fixture;
using SharedFixture.Models;

public class FilterConditionTests
{
    [Fact]
    public void Constructor_NullPropertyPath_ThrowsArgumentNullException()
    {
        PropertyPath property = null!;
        var @operator = ComparisonOperator.Contains;
        var value = "Smartphone";

        var ex = Assert.Throws<ArgumentNullException>(() => new FilterCondition(property, @operator, value));

        Assert.Equal(nameof(property), ex.ParamName);
    }

    [Fact]
    public void Negate_ReturnsNegatedCondition()
    {
        var condition = new FilterCondition(PropertyPathFactory.Create("Name"), ComparisonOperator.Contains, "Smartphone");

        var result = condition.Negate();

        Assert.IsNotType<FilterCondition>(result);
        Assert.Contains("Not(", result.ToExpression<Product>().ToString());
    }

    [Fact]
    public void Negate_CalledTwice_ReturnsOriginalCondition()
    {
        var original = new FilterCondition(PropertyPathFactory.Create("Name"), ComparisonOperator.Contains, "Smartphone");

        var doubleNegated = original.Negate().Negate();

        Assert.Equal(original.ToExpression<Product>().ToString(), doubleNegated.ToExpression<Product>().ToString());
    }

    [Fact]
    public void ToExpression_ReturnsExpectedExpression()
    {
        var property = PropertyPathFactory.Create("Name");
        var @operator = ComparisonOperator.Contains;
        var value = "Smartphone";
        var condition = new FilterCondition(property, @operator, value);

        var result = condition.ToExpression<Product>();

        Assert.Contains("Name.Contains(", result.ToString());
    }

    [Fact]
    public void ToExpression_WithQuantifierOnScalarProperty_ThrowsNotSupportedException()
    {
        var property = PropertyPathFactory.Create("Products.Reviews.Rating:any"); // Rating is scalar
        var @operator = ComparisonOperator.Equal;
        var value = DateTime.UtcNow;
        var condition = new FilterCondition(property, @operator, value);

        var ex = Assert.Throws<NotSupportedException>(() => condition.ToExpression<Category>());

        Assert.Equal("Quantifier modes cannot be applied to scalar properties.", ex.Message);
    }

    [Fact]
    public void ToExpression_WithProjectionOnScalarProperty_ThrowsNotSupportedException()
    {
        var property = PropertyPathFactory.Create("Products.Reviews.Rating:count"); // Rating is scalar
        var @operator = ComparisonOperator.Equal;
        var value = DateTime.UtcNow;
        var condition = new FilterCondition(property, @operator, value);

        var ex = Assert.Throws<NotSupportedException>(() => condition.ToExpression<Category>());

        Assert.Equal("Collection projections cannot be applied to scalar properties.", ex.Message);
    }
}
