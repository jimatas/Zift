namespace Zift.Querying.ExpressionBuilding;

using Fixture;
using Model;

public sealed class ExpressionBuilderBasicComparisonTests
{
    [Fact]
    public void Build_EqualComparisonWithInt32_EvaluatesCorrectly()
    {
        var builder = CreateBuilder();

        var expr = builder.Build(
            new ComparisonNode(
                new PropertyPathNode([nameof(TestClass.Int32Value)]),
                ComparisonOperator.Equal,
                new NumberLiteral(10)));

        var predicate = expr.Compile();

        Assert.True(predicate(new TestClass { Int32Value = 10 }));
        Assert.False(predicate(new TestClass { Int32Value = 5 }));
    }

    [Fact]
    public void Build_NotEqualComparisonWithInt32_EvaluatesCorrectly()
    {
        var builder = CreateBuilder();

        var expr = builder.Build(
            new ComparisonNode(
                new PropertyPathNode([nameof(TestClass.Int32Value)]),
                ComparisonOperator.NotEqual,
                new NumberLiteral(10)));

        var predicate = expr.Compile();

        Assert.True(predicate(new TestClass { Int32Value = 5 }));
        Assert.False(predicate(new TestClass { Int32Value = 10 }));
    }

    [Fact]
    public void Build_GreaterThanComparisonWithInt32_EvaluatesCorrectly()
    {
        var builder = CreateBuilder();

        var expr = builder.Build(
            new ComparisonNode(
                new PropertyPathNode([nameof(TestClass.Int32Value)]),
                ComparisonOperator.GreaterThan,
                new NumberLiteral(10)));

        var predicate = expr.Compile();

        Assert.True(predicate(new TestClass { Int32Value = 20 }));
        Assert.False(predicate(new TestClass { Int32Value = 10 }));
        Assert.False(predicate(new TestClass { Int32Value = 5 }));
    }

    [Fact]
    public void Build_GreaterThanOrEqualComparisonWithInt32_EvaluatesCorrectly()
    {
        var builder = CreateBuilder();

        var expr = builder.Build(
            new ComparisonNode(
                new PropertyPathNode([nameof(TestClass.Int32Value)]),
                ComparisonOperator.GreaterThanOrEqual,
                new NumberLiteral(10)));

        var predicate = expr.Compile();

        Assert.True(predicate(new TestClass { Int32Value = 10 }));
        Assert.True(predicate(new TestClass { Int32Value = 20 }));
        Assert.False(predicate(new TestClass { Int32Value = 5 }));
    }

    [Fact]
    public void Build_LessThanComparisonWithInt32_EvaluatesCorrectly()
    {
        var builder = CreateBuilder();

        var expr = builder.Build(
            new ComparisonNode(
                new PropertyPathNode([nameof(TestClass.Int32Value)]),
                ComparisonOperator.LessThan,
                new NumberLiteral(10)));

        var predicate = expr.Compile();

        Assert.True(predicate(new TestClass { Int32Value = 5 }));
        Assert.False(predicate(new TestClass { Int32Value = 10 }));
        Assert.False(predicate(new TestClass { Int32Value = 20 }));
    }

    [Fact]
    public void Build_LessThanOrEqualComparisonWithInt32_EvaluatesCorrectly()
    {
        var builder = CreateBuilder();

        var expr = builder.Build(
            new ComparisonNode(
                new PropertyPathNode([nameof(TestClass.Int32Value)]),
                ComparisonOperator.LessThanOrEqual,
                new NumberLiteral(10)));

        var predicate = expr.Compile();

        Assert.True(predicate(new TestClass { Int32Value = 10 }));
        Assert.True(predicate(new TestClass { Int32Value = 5 }));
        Assert.False(predicate(new TestClass { Int32Value = 20 }));
    }

    [Fact]
    public void Build_OrderedComparisonWithDecimal_EvaluatesCorrectly()
    {
        var builder = CreateBuilder();

        var expr = builder.Build(
            new ComparisonNode(
                new PropertyPathNode([nameof(TestClass.DecimalValue)]),
                ComparisonOperator.GreaterThan,
                new NumberLiteral(100)));

        var predicate = expr.Compile();

        Assert.True(predicate(new TestClass { DecimalValue = 150m }));
        Assert.False(predicate(new TestClass { DecimalValue = 100m }));
        Assert.False(predicate(new TestClass { DecimalValue = 50m }));
    }

    [Fact]
    public void Build_OrderedComparisonWithGuid_EvaluatesCorrectly()
    {
        var builder = CreateBuilder();

        var lower = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var higher = Guid.Parse("00000000-0000-0000-0000-000000000002");

        var expr = builder.Build(
            new ComparisonNode(
                new PropertyPathNode([nameof(TestClass.GuidValue)]),
                ComparisonOperator.GreaterThan,
                new StringLiteral(lower.ToString())));

        var predicate = expr.Compile();

        Assert.True(predicate(new TestClass { GuidValue = higher }));
        Assert.False(predicate(new TestClass { GuidValue = lower }));
    }

    [Fact]
    public void Build_OrderedComparisonWithNullableGuid_EvaluatesCorrectly()
    {
        var builder = CreateBuilder();

        var lower = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var higher = Guid.Parse("00000000-0000-0000-0000-000000000002");

        var expr = builder.Build(
            new ComparisonNode(
                new PropertyPathNode([nameof(TestClass.NullableGuidValue)]),
                ComparisonOperator.LessThan,
                new StringLiteral(higher.ToString())));

        var predicate = expr.Compile();

        Assert.True(predicate(new TestClass { NullableGuidValue = lower }));
        Assert.False(predicate(new TestClass { NullableGuidValue = higher }));
        Assert.False(predicate(new TestClass { NullableGuidValue = null }));
    }

    [Fact]
    public void Build_OrderedComparisonWithEnum_ThrowsNotSupportedException()
    {
        var builder = CreateBuilder();

        var ex = Assert.Throws<NotSupportedException>(() =>
            builder.Build(
                new ComparisonNode(
                    new PropertyPathNode([nameof(TestClass.EnumValue)]),
                    ComparisonOperator.GreaterThan,
                    new StringLiteral(nameof(TestEnum.B)))));

        Assert.Contains("is not supported for", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_OrderedComparisonWithBoolean_ThrowsNotSupportedException()
    {
        var builder = CreateBuilder();

        var ex = Assert.Throws<NotSupportedException>(() =>
            builder.Build(
                new ComparisonNode(
                    new PropertyPathNode([nameof(TestClass.BooleanValue)]),
                    ComparisonOperator.GreaterThan,
                    new BooleanLiteral(true))));

        Assert.Contains("is not supported for", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_OrderedComparisonWithNullableBoolean_ThrowsNotSupportedException()
    {
        var builder = CreateBuilder();

        var ex = Assert.Throws<NotSupportedException>(() =>
            builder.Build(
                new ComparisonNode(
                    new PropertyPathNode([nameof(TestClass.NullableBooleanValue)]),
                    ComparisonOperator.LessThan,
                    new BooleanLiteral(false))));

        Assert.Contains("is not supported for", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static ExpressionBuilder<TestClass> CreateBuilder() =>
        new(new ExpressionBuilderOptions
        {
            EnableNullGuards = false,
            ParameterizeValues = false
        });
}
