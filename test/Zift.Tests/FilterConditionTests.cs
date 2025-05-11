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
        var @operator = new ComparisonOperator(ComparisonOperatorType.Contains);
        var value = "Smartphone";

        var ex = Assert.Throws<ArgumentNullException>(() => new FilterCondition(property, @operator, value));

        Assert.Equal(nameof(property), ex.ParamName);
    }

    [Fact]
    public void Constructor_InOperatorWithNonListValue_ThrowsArgumentException()
    {
        var property = PropertyPathFactory.Create("Name");
        var @operator = new ComparisonOperator(ComparisonOperatorType.In);
        var value = "Smartphone";

        var ex = Assert.Throws<ArgumentException>(() => new FilterCondition(property, @operator, value));

        Assert.Equal("value", ex.ParamName);
        Assert.Contains("requires a list", ex.Message);
    }

    [Fact]
    public void Constructor_NonInOperatorWithListValue_ThrowsArgumentException()
    {
        var property = PropertyPathFactory.Create("Name");
        var @operator = new ComparisonOperator(ComparisonOperatorType.Contains);
        var value = new[] { "A", "B" };

        var ex = Assert.Throws<ArgumentException>(() => new FilterCondition(property, @operator, value));

        Assert.Equal("value", ex.ParamName);
        Assert.Contains("does not support a list", ex.Message);
    }

    [Fact]
    public void Constructor_InOperatorWithListValue_DoesNotThrow()
    {
        var property = PropertyPathFactory.Create("Name");
        var @operator = new ComparisonOperator(ComparisonOperatorType.In);
        var value = new[] { "A", "B" };

        var exception = Record.Exception(() => new FilterCondition(property, @operator, value));

        Assert.Null(exception);
    }

    [Fact]
    public void Constructor_NonInOperatorWithScalarValue_DoesNotThrow()
    {
        var property = PropertyPathFactory.Create("Name");
        var @operator = new ComparisonOperator(ComparisonOperatorType.Contains);
        var value = "Smartphone";

        var exception = Record.Exception(() => new FilterCondition(property, @operator, value));

        Assert.Null(exception);
    }

    [Fact]
    public void Negate_ReturnsNegatedCondition()
    {
        var condition = new FilterCondition(PropertyPathFactory.Create("Name"), new(ComparisonOperatorType.Contains), "Smartphone");

        var result = condition.Negate();

        Assert.IsNotType<FilterCondition>(result);
        Assert.Contains("Not(", result.ToExpression<Product>().ToString());
    }

    [Fact]
    public void Negate_CalledTwice_ReturnsOriginalCondition()
    {
        var original = new FilterCondition(PropertyPathFactory.Create("Name"), new(ComparisonOperatorType.Contains), "Smartphone");

        var doubleNegated = original.Negate().Negate();

        Assert.Equal(original.ToExpression<Product>().ToString(), doubleNegated.ToExpression<Product>().ToString());
    }

    [Fact]
    public void ToExpression_ReturnsExpectedExpression()
    {
        var property = PropertyPathFactory.Create("Name");
        var @operator = new ComparisonOperator(ComparisonOperatorType.Contains);
        var value = "Smartphone";
        var condition = new FilterCondition(property, @operator, value);

        var result = condition.ToExpression<Product>();

        Assert.Contains("Name.Contains(", result.ToString());
    }

    [Fact]
    public void ToExpression_WithQuantifierOnScalarProperty_ThrowsNotSupportedException()
    {
        var property = PropertyPathFactory.Create("Products.Reviews.Rating:any"); // Rating is scalar
        var @operator = new ComparisonOperator(ComparisonOperatorType.Equal);
        var value = DateTime.UtcNow;
        var condition = new FilterCondition(property, @operator, value);

        var ex = Assert.Throws<NotSupportedException>(() => condition.ToExpression<Category>());

        Assert.Equal("Quantifier modes cannot be applied to scalar properties.", ex.Message);
    }

    [Fact]
    public void ToExpression_WithProjectionOnScalarProperty_ThrowsNotSupportedException()
    {
        var property = PropertyPathFactory.Create("Products.Reviews.Rating:count"); // Rating is scalar
        var @operator = new ComparisonOperator(ComparisonOperatorType.Equal);
        var value = DateTime.UtcNow;
        var condition = new FilterCondition(property, @operator, value);

        var ex = Assert.Throws<NotSupportedException>(() => condition.ToExpression<Category>());

        Assert.Equal("Collection projections cannot be applied to scalar properties.", ex.Message);
    }

    [Fact]
    public void ToExpression_WhenParameterizeValuesDisabled_UsesConstantExpression()
    {
        var property = PropertyPathFactory.Create("Price");
        var @operator = new ComparisonOperator(ComparisonOperatorType.Equal);
        var condition = new FilterCondition(property, @operator, 123.45);
        var options = new FilterOptions
        {
            ParameterizeValues = false,
            EnableNullGuards = false // Disable null guards for this test.
        };

        var expression = condition.ToExpression<Product>(options);

        var binaryExpr = Assert.IsType<BinaryExpression>(expression.Body, exactMatch: false);
        Assert.IsType<ConstantExpression>(binaryExpr.Right);
    }

    [Fact]
    public void ToExpression_WhenParameterizeValuesEnabled_UsesWrappedValueExpression()
    {
        var property = PropertyPathFactory.Create("Price");
        var @operator = new ComparisonOperator(ComparisonOperatorType.Equal);
        var condition = new FilterCondition(property, @operator, 123.45);
        var options = new FilterOptions
        {
            ParameterizeValues = true,
            EnableNullGuards = false // Disable null guards for this test.
        };

        var expression = condition.ToExpression<Product>(options);

        var binaryExpr = Assert.IsType<BinaryExpression>(expression.Body, exactMatch: false);
        var convertExpr = Assert.IsType<UnaryExpression>(binaryExpr.Right);
        Assert.IsType<MemberExpression>(convertExpr.Operand, exactMatch: false);
    }
}
