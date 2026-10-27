using Zift.Fixture;
using Zift.Filtering.Model;

namespace Zift.Filtering.ExpressionBuilding;

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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Build_InComparisonWithNullableInt32_EmptyList_MatchesNothing(bool enableNullGuards)
    {
        var predicate = BuildIn(nameof(TestClass.NullableInt32Value), [], enableNullGuards);

        Assert.False(predicate(new TestClass { NullableInt32Value = null }));
        Assert.False(predicate(new TestClass { NullableInt32Value = 0 }));
        Assert.False(predicate(new TestClass { NullableInt32Value = 1 }));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Build_InComparisonWithString_EmptyList_MatchesNothing(bool enableNullGuards)
    {
        var predicate = BuildIn(nameof(TestClass.StringValue), [], enableNullGuards);

        Assert.False(predicate(new TestClass { StringValue = null }));
        Assert.False(predicate(new TestClass { StringValue = string.Empty }));
        Assert.False(predicate(new TestClass { StringValue = "a" }));
    }

    [Theory]
    [InlineData(nameof(TestClass.NullableBooleanValue))]
    [InlineData(nameof(TestClass.NullableEnumValue))]
    [InlineData(nameof(TestClass.NullableGuidValue))]
    public void Build_InComparisonWithNullableValueType_EmptyList_DoesNotMatchNull(string property)
    {
        foreach (var enableNullGuards in new[] { true, false })
        {
            var predicate = BuildIn(property, [], enableNullGuards);

            Assert.False(predicate(new TestClass()));
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Build_InComparisonWithNestedNullableProperty_EmptyList_MatchesNothing(bool enableNullGuards)
    {
        var predicate = BuildIn(
            [nameof(TestClass.NestedEntity), nameof(TestClass.NullableInt32Value)],
            [],
            enableNullGuards);

        Assert.False(predicate(new TestClass { NestedEntity = new TestClass { NullableInt32Value = null } }));
        Assert.False(predicate(new TestClass { NestedEntity = new TestClass { NullableInt32Value = 1 } }));

        if (enableNullGuards)
        {
            Assert.False(predicate(new TestClass { NestedEntity = null }));
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Build_InComparisonWithNullableString_ListContainsOnlyNull_MatchesOnlyNull(bool enableNullGuards)
    {
        var predicate = BuildIn(nameof(TestClass.StringValue), [new NullLiteral()], enableNullGuards);

        Assert.True(predicate(new TestClass { StringValue = null }));
        Assert.False(predicate(new TestClass { StringValue = string.Empty }));
        Assert.False(predicate(new TestClass { StringValue = "a" }));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Build_NegatedInComparison_EmptyList_MatchesEverything(bool enableNullGuards)
    {
        var builder = CreateBuilder(enableNullGuards);

        var predicate = builder.Build(
            new NotNode(
                new ComparisonNode(
                    new PropertyPathNode([nameof(TestClass.NullableInt32Value)]),
                    ComparisonOperator.In,
                    new ListLiteral([])))).Compile();

        Assert.True(predicate(new TestClass { NullableInt32Value = null }));
        Assert.True(predicate(new TestClass { NullableInt32Value = 1 }));
    }

    [Fact]
    public void Build_InComparisonWithNullableInt32_PropertyIsNull_ReturnsFalse()
    {
        var predicate = BuildNullableInt32In([1, 2]);

        Assert.False(predicate(new TestClass { NullableInt32Value = null }));
    }

    [Fact]
    public void Build_InComparisonWithInt32_ListContainsNull_ThrowsInvalidFilterException()
    {
        var builder = CreateBuilder();

        Assert.Throws<InvalidFilterException>(() =>
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

    private static Func<TestClass, bool> BuildIn(
        string property,
        IReadOnlyList<LiteralNode> items,
        bool enableNullGuards) =>
        BuildIn([property], items, enableNullGuards);

    private static Func<TestClass, bool> BuildIn(
        string[] path,
        IReadOnlyList<LiteralNode> items,
        bool enableNullGuards)
    {
        var builder = CreateBuilder(enableNullGuards);

        var expr = builder.Build(
            new ComparisonNode(
                new PropertyPathNode(path),
                ComparisonOperator.In,
                new ListLiteral(items)));

        return expr.Compile();
    }

    private static ExpressionBuilder<TestClass> CreateBuilder(bool enableNullGuards = true) =>
        new(new ExpressionBuilderOptions
        {
            EnableNullGuards = enableNullGuards,
            ParameterizeValues = false
        });
}
