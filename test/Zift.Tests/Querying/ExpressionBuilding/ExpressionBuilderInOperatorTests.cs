namespace Zift.Querying.ExpressionBuilding;

using Fixture;
using Model;

public sealed class ExpressionBuilderInOperatorTests
{
    [Fact]
    public void Build_InComparisonWithInt32_ValueInList_EvaluatesCorrectly()
    {
        var predicate = BuildInt32In([1, 2, 3]);

        Assert.True(predicate(new TestClass { Int32Value = 2 }));
        Assert.False(predicate(new TestClass { Int32Value = 5 }));
    }

    [Fact]
    public void Build_InComparisonWithInt32_EmptyList_ReturnsFalse()
    {
        var predicate = BuildInt32In([]);

        Assert.False(predicate(new TestClass { Int32Value = 1 }));
    }

    [Fact]
    public void Build_InComparisonWithNullableInt32_ListContainsNull_EvaluatesCorrectly()
    {
        var predicate = BuildNullableInt32In([null, 1, 2]);

        Assert.True(predicate(new TestClass { NullableInt32Value = null }));
        Assert.False(predicate(new TestClass { NullableInt32Value = 3 }));
    }

    [Fact]
    public void Build_InComparisonWithNullableInt32_ValueInList_EvaluatesCorrectly()
    {
        var predicate = BuildNullableInt32In([1, 2, 3]);

        Assert.True(predicate(new TestClass { NullableInt32Value = 2 }));
        Assert.False(predicate(new TestClass { NullableInt32Value = 4 }));
    }

    [Fact]
    public void Build_InComparisonWithNullableInt32_ListContainsOnlyNull_EvaluatesCorrectly()
    {
        var predicate = BuildNullableInt32In([null]);

        var entity = new TestClass { NullableInt32Value = null };

        var ex = Record.Exception(() => predicate(entity));

        Assert.Null(ex);
        Assert.True(predicate(entity));
    }

    [Fact]
    public void Build_InComparisonWithNullableInt32_PropertyIsNull_ReturnsFalse()
    {
        var predicate = BuildNullableInt32In([1, 2]);

        Assert.False(predicate(new TestClass { NullableInt32Value = null }));
    }

    [Fact]
    public void Build_InComparisonWithInt32_ListContainsNull_ThrowsInvalidOperationException()
    {
        var builder = CreateBuilder();

        Assert.Throws<InvalidOperationException>(() =>
            builder.Build(
                new ComparisonNode(
                    new PropertyPathNode([nameof(TestClass.Int32Value)]),
                    ComparisonOperator.In,
                    new ListLiteral(
                    [
                        new NullLiteral(),
                        new NumberLiteral(1)
                    ]))));
    }

    private static Func<TestClass, bool> BuildInt32In(IEnumerable<int> values)
    {
        var builder = CreateBuilder();

        var expr = builder.Build(
            new ComparisonNode(
                new PropertyPathNode([nameof(TestClass.Int32Value)]),
                ComparisonOperator.In,
                new ListLiteral(
                    values.Select(v => new NumberLiteral(v)).ToList())));

        return expr.Compile();
    }

    private static Func<TestClass, bool> BuildNullableInt32In(IEnumerable<int?> values)
    {
        var builder = CreateBuilder();

        var literals = values
            .Select(v => v is null
                ? (LiteralNode)new NullLiteral()
                : new NumberLiteral(v.Value))
            .ToList();

        var expr = builder.Build(
            new ComparisonNode(
                new PropertyPathNode([nameof(TestClass.NullableInt32Value)]),
                ComparisonOperator.In,
                new ListLiteral(literals)));

        return expr.Compile();
    }

    private static ExpressionBuilder<TestClass> CreateBuilder() =>
        new(new ExpressionBuilderOptions
        {
            EnableNullGuards = true,
            ParameterizeValues = false
        });
}
