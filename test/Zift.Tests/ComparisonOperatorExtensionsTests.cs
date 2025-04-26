namespace Zift.Tests;

using Filtering.Dynamic;

public class ComparisonOperatorExtensionsTests
{
    [Fact]
    public void ToComparisonExpression_InvalidOperator_ThrowsNotSupportedException()
    {
        var leftOperand = Expression.Constant(1);
        var rightOperand = Expression.Constant(1);
        var @operator = (ComparisonOperator)int.MaxValue;

        var ex = Assert.Throws<NotSupportedException>(() => _ = @operator.ToComparisonExpression(leftOperand, rightOperand));
        Assert.Equal($"The operator '{@operator}' is not supported.", ex.Message);
    }

    [Fact]
    public void ToComparisonExpression_EqualOperator_ReturnsEqualExpression()
    {
        var leftOperand = Expression.Constant(1);
        var rightOperand = Expression.Constant(1);
        var @operator = ComparisonOperator.Equal;

        var result = @operator.ToComparisonExpression(leftOperand, rightOperand);

        var expected = Expression.Equal(leftOperand, rightOperand);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ToComparisonExpression_NotEqualOperator_ReturnsNotEqualExpression()
    {
        var leftOperand = Expression.Constant(1);
        var rightOperand = Expression.Constant(1);
        var @operator = ComparisonOperator.NotEqual;

        var result = @operator.ToComparisonExpression(leftOperand, rightOperand);

        var expected = Expression.NotEqual(leftOperand, rightOperand);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ToComparisonExpression_GreaterThanOperator_ReturnsGreaterThanExpression()
    {
        var leftOperand = Expression.Constant(1);
        var rightOperand = Expression.Constant(1);
        var @operator = ComparisonOperator.GreaterThan;

        var result = @operator.ToComparisonExpression(leftOperand, rightOperand);

        var expected = Expression.GreaterThan(leftOperand, rightOperand);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ToComparisonExpression_GreaterThanOrEqualOperator_ReturnsGreaterThanOrEqualExpression()
    {
        var leftOperand = Expression.Constant(1);
        var rightOperand = Expression.Constant(1);
        var @operator = ComparisonOperator.GreaterThanOrEqual;

        var result = @operator.ToComparisonExpression(leftOperand, rightOperand);

        var expected = Expression.GreaterThanOrEqual(leftOperand, rightOperand);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ToComparisonExpression_LessThanOperator_ReturnsLessThanExpression()
    {
        var leftOperand = Expression.Constant(1);
        var rightOperand = Expression.Constant(1);
        var @operator = ComparisonOperator.LessThan;

        var result = @operator.ToComparisonExpression(leftOperand, rightOperand);

        var expected = Expression.LessThan(leftOperand, rightOperand);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ToComparisonExpression_LessThanOrEqualOperator_ReturnsLessThanOrEqualExpression()
    {
        var leftOperand = Expression.Constant(1);
        var rightOperand = Expression.Constant(1);
        var @operator = ComparisonOperator.LessThanOrEqual;

        var result = @operator.ToComparisonExpression(leftOperand, rightOperand);

        var expected = Expression.LessThanOrEqual(leftOperand, rightOperand);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ToComparisonExpression_ContainsOperatorWithStringOperands_ReturnsContainsExpression()
    {
        var leftOperand = Expression.Constant("abc");
        var rightOperand = Expression.Constant("a");
        var @operator = ComparisonOperator.Contains;

        var result = @operator.ToComparisonExpression(leftOperand, rightOperand);

        var expected = Expression.Call(leftOperand, GetComparisonMethod(nameof(string.Contains)), rightOperand);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ToComparisonExpression_StartsWithOperatorWithStringOperands_ReturnsStartsWithExpression()
    {
        var leftOperand = Expression.Constant("abc");
        var rightOperand = Expression.Constant("a");
        var @operator = ComparisonOperator.StartsWith;

        var result = @operator.ToComparisonExpression(leftOperand, rightOperand);

        var expected = Expression.Call(leftOperand, GetComparisonMethod(nameof(string.StartsWith)), rightOperand);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ToComparisonExpression_EndsWithOperatorWithStringOperands_ReturnsEndsWithExpression()
    {
        var leftOperand = Expression.Constant("abc");
        var rightOperand = Expression.Constant("a");
        var @operator = ComparisonOperator.EndsWith;

        var result = @operator.ToComparisonExpression(leftOperand, rightOperand);

        var expected = Expression.Call(leftOperand, GetComparisonMethod(nameof(string.EndsWith)), rightOperand);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    private static MethodInfo GetComparisonMethod(string name)
    {
        return typeof(string)
            .GetMethods()
            .Single(method =>
                method.Name == name
                && method.GetParameters().Length == 1
                && method.GetParameters().Single().ParameterType == typeof(string));
    }
}
