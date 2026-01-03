namespace Zift.Querying.ExpressionBuilding;

using Fixture;
using Model;

public sealed class ExpressionBuilderLogicalTests
{
    [Fact]
    public void Build_AndExpression_AllTermsMustMatch()
    {
        var predicate = BuildAnd(
            GreaterThan(nameof(TestClass.Int32Value), 5),
            LessThan(nameof(TestClass.Int32Value), 10));

        Assert.True(predicate(new TestClass { Int32Value = 7 }));
        Assert.False(predicate(new TestClass { Int32Value = 3 }));
        Assert.False(predicate(new TestClass { Int32Value = 12 }));
    }

    [Fact]
    public void Build_OrExpression_AnyTermMayMatch()
    {
        var predicate = BuildOr(
            Equal(nameof(TestClass.Int32Value), 5),
            Equal(nameof(TestClass.Int32Value), 10));

        Assert.True(predicate(new TestClass { Int32Value = 5 }));
        Assert.True(predicate(new TestClass { Int32Value = 10 }));
        Assert.False(predicate(new TestClass { Int32Value = 7 }));
    }

    [Fact]
    public void Build_NotExpression_InvertsPredicate()
    {
        var predicate = BuildNot(
            Equal(nameof(TestClass.Int32Value), 5));

        Assert.False(predicate(new TestClass { Int32Value = 5 }));
        Assert.True(predicate(new TestClass { Int32Value = 6 }));
    }

    [Fact]
    public void Build_NestedLogicalExpression_EvaluatesCorrectly()
    {
        var predicate = BuildOr(
            new LogicalNode(
                LogicalOperator.And,
                [
                    GreaterThan(nameof(TestClass.Int32Value), 5),
                    LessThan(nameof(TestClass.Int32Value), 10)
                ]),
            Equal(nameof(TestClass.Int32Value), 20));

        Assert.True(predicate(new TestClass { Int32Value = 7 }));
        Assert.True(predicate(new TestClass { Int32Value = 20 }));
        Assert.False(predicate(new TestClass { Int32Value = 3 }));
        Assert.False(predicate(new TestClass { Int32Value = 15 }));
    }

    [Fact]
    public void Build_LogicalExpressionWithSingleTerm_EvaluatesCorrectly()
    {
        var predicate = BuildAnd(
            Equal(nameof(TestClass.Int32Value), 5));

        Assert.True(predicate(new TestClass { Int32Value = 5 }));
        Assert.False(predicate(new TestClass { Int32Value = 6 }));
    }

    private static Func<TestClass, bool> BuildAnd(params PredicateNode[] terms)
    {
        var builder = CreateBuilder();

        var expr = builder.Build(
            new LogicalNode(LogicalOperator.And, terms));

        return expr.Compile();
    }

    private static Func<TestClass, bool> BuildOr(params PredicateNode[] terms)
    {
        var builder = CreateBuilder();

        var expr = builder.Build(
            new LogicalNode(LogicalOperator.Or, terms));

        return expr.Compile();
    }

    private static Func<TestClass, bool> BuildNot(PredicateNode inner)
    {
        var builder = CreateBuilder();

        var expr = builder.Build(
            new NotNode(inner));

        return expr.Compile();
    }

    private static PredicateNode Equal(string property, int value) =>
        new ComparisonNode(
            new PropertyPathNode([property]),
            ComparisonOperator.Equal,
            new NumberLiteral(value));

    private static PredicateNode GreaterThan(string property, int value) =>
        new ComparisonNode(
            new PropertyPathNode([property]),
            ComparisonOperator.GreaterThan,
            new NumberLiteral(value));

    private static PredicateNode LessThan(string property, int value) =>
        new ComparisonNode(
            new PropertyPathNode([property]),
            ComparisonOperator.LessThan,
            new NumberLiteral(value));

    private static ExpressionBuilder<TestClass> CreateBuilder() =>
        new(new ExpressionBuilderOptions
        {
            EnableNullGuards = true,
            ParameterizeValues = false
        });
}
