namespace Zift.Tests;

using Filtering.Dynamic;
using Fixture;

public class ComparisonOperatorTypeTests
{
    [Theory]
    [ClassData(typeof(ComparisonOperatorData))]
    public void TryParse_ValidOperator_ReturnsTrueAndOperator(string symbol, ComparisonOperatorType expectedResult)
    {
        var result = ComparisonOperatorType.TryParse(symbol, out var @operator);

        Assert.True(result);
        Assert.Equal(expectedResult, @operator);
    }

    [Theory]
    [InlineData("~=")]
    [InlineData("invalid")]
    public void TryParse_InvalidOperator_ReturnsFalse(string symbol)
    {
        var result = ComparisonOperatorType.TryParse(symbol, out var @operator);

        Assert.False(result);
        Assert.Equal(default, @operator);
    }

    [Fact]
    public void ToComparisonExpression_InvalidOperator_ThrowsNotSupportedException()
    {
        var leftOperand = Expression.Constant(1);
        var rightOperand = Expression.Constant(1);
        var @operator = new ComparisonOperatorType("~=");

        var ex = Assert.Throws<NotSupportedException>(() => _ = @operator.ToComparisonExpression(leftOperand, rightOperand));
        Assert.StartsWith($"The operator '{@operator}' is not supported", ex.Message);
    }

    [Fact]
    public void ToComparisonExpression_EqualOperator_ReturnsEqualExpression()
    {
        var leftOperand = Expression.Constant(1);
        var rightOperand = Expression.Constant(1);
        var @operator = ComparisonOperatorType.Equal;

        var result = @operator.ToComparisonExpression(leftOperand, rightOperand);

        var expected = Expression.Equal(leftOperand, rightOperand);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ToComparisonExpression_NotEqualOperator_ReturnsNotEqualExpression()
    {
        var leftOperand = Expression.Constant(1);
        var rightOperand = Expression.Constant(1);
        var @operator = ComparisonOperatorType.NotEqual;

        var result = @operator.ToComparisonExpression(leftOperand, rightOperand);

        var expected = Expression.NotEqual(leftOperand, rightOperand);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ToComparisonExpression_GreaterThanOperator_ReturnsGreaterThanExpression()
    {
        var leftOperand = Expression.Constant(1);
        var rightOperand = Expression.Constant(1);
        var @operator = ComparisonOperatorType.GreaterThan;

        var result = @operator.ToComparisonExpression(leftOperand, rightOperand);

        var expected = Expression.GreaterThan(leftOperand, rightOperand);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ToComparisonExpression_GreaterThanOrEqualOperator_ReturnsGreaterThanOrEqualExpression()
    {
        var leftOperand = Expression.Constant(1);
        var rightOperand = Expression.Constant(1);
        var @operator = ComparisonOperatorType.GreaterThanOrEqual;

        var result = @operator.ToComparisonExpression(leftOperand, rightOperand);

        var expected = Expression.GreaterThanOrEqual(leftOperand, rightOperand);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ToComparisonExpression_LessThanOperator_ReturnsLessThanExpression()
    {
        var leftOperand = Expression.Constant(1);
        var rightOperand = Expression.Constant(1);
        var @operator = ComparisonOperatorType.LessThan;

        var result = @operator.ToComparisonExpression(leftOperand, rightOperand);

        var expected = Expression.LessThan(leftOperand, rightOperand);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ToComparisonExpression_LessThanOrEqualOperator_ReturnsLessThanOrEqualExpression()
    {
        var leftOperand = Expression.Constant(1);
        var rightOperand = Expression.Constant(1);
        var @operator = ComparisonOperatorType.LessThanOrEqual;

        var result = @operator.ToComparisonExpression(leftOperand, rightOperand);

        var expected = Expression.LessThanOrEqual(leftOperand, rightOperand);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ToComparisonExpression_ContainsOperatorWithStringOperands_ReturnsContainsExpression()
    {
        var leftOperand = Expression.Constant("abc");
        var rightOperand = Expression.Constant("a");
        var @operator = ComparisonOperatorType.Contains;

        var result = @operator.ToComparisonExpression(leftOperand, rightOperand);

        var expected = Expression.Call(leftOperand, GetComparisonMethod(nameof(string.Contains)), rightOperand);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ToComparisonExpression_StartsWithOperatorWithStringOperands_ReturnsStartsWithExpression()
    {
        var leftOperand = Expression.Constant("abc");
        var rightOperand = Expression.Constant("a");
        var @operator = ComparisonOperatorType.StartsWith;

        var result = @operator.ToComparisonExpression(leftOperand, rightOperand);

        var expected = Expression.Call(leftOperand, GetComparisonMethod(nameof(string.StartsWith)), rightOperand);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ToComparisonExpression_EndsWithOperatorWithStringOperands_ReturnsEndsWithExpression()
    {
        var leftOperand = Expression.Constant("abc");
        var rightOperand = Expression.Constant("a");
        var @operator = ComparisonOperatorType.EndsWith;

        var result = @operator.ToComparisonExpression(leftOperand, rightOperand);

        var expected = Expression.Call(leftOperand, GetComparisonMethod(nameof(string.EndsWith)), rightOperand);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ToComparisonExpression_InOperatorWithArrayOperand_ReturnsInExpression()
    {
        var leftOperand = Expression.Constant(1);
        var rightOperand = Expression.Constant(new[] { 1, 2, 3 });
        var @operator = ComparisonOperatorType.In;
        
        var result = @operator.ToComparisonExpression(leftOperand, rightOperand);
        
        var expected = Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), [leftOperand.Type], rightOperand, leftOperand);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ToComparisonExpression_InOperatorWithNonArrayOperand_ThrowsNotSupportedException()
    {
        var leftOperand = Expression.Constant(1);
        var rightOperand = Expression.Constant(1);
        var @operator = ComparisonOperatorType.In;

        var ex = Assert.Throws<NotSupportedException>(() => _ = @operator.ToComparisonExpression(leftOperand, rightOperand));
        Assert.StartsWith($"The operator '{@operator}' is not supported", ex.Message);
    }

    [Fact]
    public void ToComparisonExpression_ContainsOperatorWithNonStringOperands_ThrowsNotSupportedException()
    {
        var leftOperand = Expression.Constant(1);
        var rightOperand = Expression.Constant(1);
        var @operator = ComparisonOperatorType.Contains;

        var ex = Assert.Throws<NotSupportedException>(() => _ = @operator.ToComparisonExpression(leftOperand, rightOperand));
        Assert.StartsWith($"The operator '{@operator}' is not supported", ex.Message);
    }

    [Fact]
    public void ToComparisonExpression_StartsWithOperatorWithNonStringOperands_ThrowsNotSupportedException()
    {
        var leftOperand = Expression.Constant(1);
        var rightOperand = Expression.Constant(1);
        var @operator = ComparisonOperatorType.StartsWith;
        
        var ex = Assert.Throws<NotSupportedException>(() => _ = @operator.ToComparisonExpression(leftOperand, rightOperand));
        Assert.StartsWith($"The operator '{@operator}' is not supported", ex.Message);
    }

    [Fact]
    public void ToComparisonExpression_EndsWithOperatorWithNonStringOperands_ThrowsNotSupportedException()
    {
        var leftOperand = Expression.Constant(1);
        var rightOperand = Expression.Constant(1);
        var @operator = ComparisonOperatorType.EndsWith;
        
        var ex = Assert.Throws<NotSupportedException>(() => _ = @operator.ToComparisonExpression(leftOperand, rightOperand));
        Assert.StartsWith($"The operator '{@operator}' is not supported", ex.Message);
    }

    private static MethodInfo GetComparisonMethod(string name)
    {
        return typeof(string)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(method => method.Name == name
                && method.GetParameters().Length == 1
                && method.GetParameters().Single().ParameterType == typeof(string));
    }
}
