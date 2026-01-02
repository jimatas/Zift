namespace Zift.Querying.ExpressionBuilding;

using Fixture;
using Model;

public sealed class ExpressionBuilderParameterizationTests
{
    [Fact]
    public void Build_WithParameterizationDisabled_EmbedsConstantValue()
    {
        var expr = BuildExpression(
            parameterize: false,
            ComparisonOperator.GreaterThan,
            new NumberLiteral(5));

        Assert.Contains("5", expr.Body.ToString());
    }

    [Fact]
    public void Build_WithParameterizationEnabled_DoesNotEmbedConstantValues()
    {
        var expr = BuildExpression(
            parameterize: true,
            ComparisonOperator.GreaterThan,
            new NumberLiteral(5));

        Assert.DoesNotContain("5", expr.Body.ToString());
    }

    [Fact]
    public void Build_WithParameterizationEnabled_PreservesEvaluationBehavior()
    {
        var predicate = BuildPredicate(
            parameterize: true,
            ComparisonOperator.GreaterThan,
            new NumberLiteral(5));

        Assert.True(predicate(new TestClass { Int32Value = 10 }));
        Assert.False(predicate(new TestClass { Int32Value = 3 }));
    }

    [Fact]
    public void Build_InComparisonWithParameterizationEnabled_PreservesEvaluationBehavior()
    {
        var predicate = BuildPredicate(
            parameterize: true,
            ComparisonOperator.In,
            new ListLiteral(
            [
                new NumberLiteral(1),
                new NumberLiteral(2),
                new NumberLiteral(3)
            ]));

        Assert.True(predicate(new TestClass { Int32Value = 2 }));
        Assert.False(predicate(new TestClass { Int32Value = 5 }));
    }

    private static Expression<Func<TestClass, bool>> BuildExpression(
        bool parameterize,
        ComparisonOperator op,
        LiteralNode literal)
    {
        var builder = new ExpressionBuilder<TestClass>(
            new ExpressionBuilderOptions
            {
                EnableNullGuards = false,
                ParameterizeValues = parameterize
            });

        return builder.Build(
            new ComparisonNode(
                new PropertyPathNode([nameof(TestClass.Int32Value)]),
                op,
                literal));
    }

    private static Func<TestClass, bool> BuildPredicate(
        bool parameterize,
        ComparisonOperator op,
        LiteralNode literal)
    {
        return BuildExpression(parameterize, op, literal).Compile();
    }
}
