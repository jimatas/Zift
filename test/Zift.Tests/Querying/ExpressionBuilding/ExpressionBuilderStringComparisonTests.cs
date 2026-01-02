namespace Zift.Querying.ExpressionBuilding;

using Fixture;
using Model;

public sealed class ExpressionBuilderStringComparisonTests
{
    [Fact]
    public void Build_EqualComparisonWithString_ReturnsTrueWhenValuesAreEqual()
    {
        var predicate = Build(
            ComparisonOperator.Equal,
            "Alice");

        Assert.True(predicate(new TestClass { StringValue = "Alice" }));
        Assert.False(predicate(new TestClass { StringValue = "Bob" }));
    }

    [Fact]
    public void Build_NotEqualComparisonWithString_ReturnsTrueWhenValuesDiffer()
    {
        var predicate = Build(
            ComparisonOperator.NotEqual,
            "Alice");

        Assert.True(predicate(new TestClass { StringValue = "Bob" }));
        Assert.False(predicate(new TestClass { StringValue = "Alice" }));
    }

    [Fact]
    public void Build_ContainsComparisonWithString_ReturnsTrueWhenSubstringIsPresent()
    {
        var predicate = Build(
            ComparisonOperator.Contains,
            "lic");

        Assert.True(predicate(new TestClass { StringValue = "Alice" }));
        Assert.False(predicate(new TestClass { StringValue = "Bob" }));
    }

    [Fact]
    public void Build_StartsWithComparisonWithString_ReturnsTrueWhenPrefixMatches()
    {
        var predicate = Build(
            ComparisonOperator.StartsWith,
            "Al");

        Assert.True(predicate(new TestClass { StringValue = "Alice" }));
        Assert.False(predicate(new TestClass { StringValue = "Malice" }));
    }

    [Fact]
    public void Build_EndsWithComparisonWithString_ReturnsTrueWhenSuffixMatches()
    {
        var predicate = Build(
            ComparisonOperator.EndsWith,
            "ice");

        Assert.True(predicate(new TestClass { StringValue = "Alice" }));
        Assert.False(predicate(new TestClass { StringValue = "Iceland" }));
    }

    [Fact]
    public void Build_GreaterThanComparisonWithString_ReturnsTrueWhenValueIsGreater()
    {
        var predicate = Build(
            ComparisonOperator.GreaterThan,
            "Bob");

        Assert.True(predicate(new TestClass { StringValue = "Charlie" }));
        Assert.False(predicate(new TestClass { StringValue = "Alice" }));
    }

    [Fact]
    public void Build_GreaterThanOrEqualComparisonWithString_ReturnsTrueWhenValueIsGreaterOrEqual()
    {
        var predicate = Build(
            ComparisonOperator.GreaterThanOrEqual,
            "Bob");

        Assert.True(predicate(new TestClass { StringValue = "Bob" }));
        Assert.True(predicate(new TestClass { StringValue = "Charlie" }));
        Assert.False(predicate(new TestClass { StringValue = "Alice" }));
    }

    [Fact]
    public void Build_LessThanComparisonWithString_ReturnsTrueWhenValueIsLess()
    {
        var predicate = Build(
            ComparisonOperator.LessThan,
            "Bob");

        Assert.True(predicate(new TestClass { StringValue = "Alice" }));
        Assert.False(predicate(new TestClass { StringValue = "Charlie" }));
    }

    [Fact]
    public void Build_LessThanOrEqualComparisonWithString_ReturnsTrueWhenValueIsLessOrEqual()
    {
        var predicate = Build(
            ComparisonOperator.LessThanOrEqual,
            "Bob");

        Assert.True(predicate(new TestClass { StringValue = "Alice" }));
        Assert.True(predicate(new TestClass { StringValue = "Bob" }));
        Assert.False(predicate(new TestClass { StringValue = "Charlie" }));
    }

    private static Func<TestClass, bool> Build(
        ComparisonOperator op,
        string value)
    {
        var builder = new ExpressionBuilder<TestClass>(
            new ExpressionBuilderOptions
            {
                EnableNullGuards = false,
                ParameterizeValues = false
            });

        var expr = builder.Build(
            new ComparisonNode(
                new PropertyPathNode([nameof(TestClass.StringValue)]),
                op,
                new StringLiteral(value)));

        return expr.Compile();
    }
}
