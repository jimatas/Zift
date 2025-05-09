namespace Zift.Tests;

using Filtering.Dynamic;
using Fixture;
using SharedFixture.Models;

public class FilterGroupTests
{
    [Fact]
    public void ToExpression_WithAndOperator_ReturnsConjunctionExpression()
    {
        var group = new FilterGroup(LogicalOperator.And);
        group.Terms.Add(new FilterCondition(PropertyPathFactory.Create("Name"), new(ComparisonOperatorType.Equal), "Smartphone"));
        group.Terms.Add(new FilterCondition(PropertyPathFactory.Create("Price"), new(ComparisonOperatorType.GreaterThan), 500));

        var result = group.ToExpression<Product>().ToString();

        Assert.Contains("p.Name == ", result);
        Assert.Contains("p.Price > ", result);
        Assert.DoesNotContain("OrElse", result);
    }

    [Fact]
    public void ToExpression_WithOrOperator_ReturnsDisjunctionExpression()
    {
        var group = new FilterGroup(LogicalOperator.Or);
        group.Terms.Add(new FilterCondition(PropertyPathFactory.Create("Name"), new(ComparisonOperatorType.Equal), "Smartphone"));
        group.Terms.Add(new FilterCondition(PropertyPathFactory.Create("Price"), new(ComparisonOperatorType.GreaterThan), 500));

        var result = group.ToExpression<Product>().ToString();

        Assert.Contains("p.Name == ", result);
        Assert.Contains("p.Price > ", result);
        Assert.Contains("OrElse", result);
    }

    [Fact]
    public void ToExpression_SingleTerm_ReturnsExpectedExpression()
    {
        var group = new FilterGroup(LogicalOperator.Or);
        group.Terms.Add(new FilterCondition(PropertyPathFactory.Create("Name"), new(ComparisonOperatorType.Equal), "Smartphone"));

        var result = group.ToExpression<Product>().ToString();

        Assert.Contains("p.Name == ", result);
        Assert.DoesNotContain("OrElse", result);
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
        group.Terms.Add(new FilterCondition(PropertyPathFactory.Create("Name"), new(ComparisonOperatorType.Equal), "Smartphone"));
        group.Terms.Add(new FilterCondition(PropertyPathFactory.Create("Price"), new(ComparisonOperatorType.GreaterThan), 500));

        var doubleNegated = group.Negate().Negate();

        Assert.IsType<FilterGroup>(doubleNegated);
        Assert.Equal(group.ToExpression<Product>().ToString(), doubleNegated.ToExpression<Product>().ToString());
    }
}
