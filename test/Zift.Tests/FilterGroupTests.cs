namespace Zift.Tests;

using Filtering.Dynamic;
using Fixture;
using SharedFixture.Models;

public class FilterGroupTests
{
    [Theory]
    [InlineData(LogicalOperator.And, "AndAlso")]
    [InlineData(LogicalOperator.Or, "OrElse")]
    public void ToExpression_TwoTerms_ReturnsExpectedExpression(LogicalOperator logicalOperator, string expectedLinqCombinator)
    {
        var group = new FilterGroup(logicalOperator);
        group.Terms.Add(new FilterCondition(PropertyPathFactory.Create("Name"), ComparisonOperator.Equal, "Smartphone"));
        group.Terms.Add(new FilterCondition(PropertyPathFactory.Create("Price"), ComparisonOperator.GreaterThan, 500));

        var result = group.ToExpression<Product>().ToString();

        Assert.Contains("p.Name == ", result);
        Assert.Contains(expectedLinqCombinator, result);
        Assert.Contains("p.Price > ", result);
    }

    [Fact]
    public void ToExpression_SingleTerm_ReturnsExpectedExpression()
    {
        var group = new FilterGroup(LogicalOperator.And);
        group.Terms.Add(new FilterCondition(PropertyPathFactory.Create("Name"), ComparisonOperator.Equal, "Smartphone"));

        var result = group.ToExpression<Product>().ToString();

        Assert.Contains("p.Name == ", result);
        Assert.DoesNotContain("AndAlso", result);
    }

    [Theory]
    [InlineData(LogicalOperator.And, "p => true")]
    [InlineData(LogicalOperator.Or, "p => false")]
    public void ToExpression_NoTerms_ReturnsDefaultExpression(LogicalOperator @operator, string defaultReturnValue)
    {
        var group = new FilterGroup(@operator);

        var result = group.ToExpression<Product>().ToString();

        Assert.Equal(defaultReturnValue, result, ignoreCase: true);
    }

    [Fact]
    public void Negate_Twice_ReturnsSemanticallyEquivalentFilterGroup()
    {
        var group = new FilterGroup(LogicalOperator.Or);
        group.Terms.Add(new FilterCondition(PropertyPathFactory.Create("Name"), ComparisonOperator.Equal, "Smartphone"));
        group.Terms.Add(new FilterCondition(PropertyPathFactory.Create("Price"), ComparisonOperator.GreaterThan, 500));

        var doubleNegated = group.Negate().Negate();

        Assert.IsType<FilterGroup>(doubleNegated);
        Assert.Equal(group.ToExpression<Product>().ToString(), doubleNegated.ToExpression<Product>().ToString());
    }
}
