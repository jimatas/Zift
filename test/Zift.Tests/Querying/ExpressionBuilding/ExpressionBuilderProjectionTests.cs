namespace Zift.Querying.ExpressionBuilding;

using Fixture;
using Model;

public sealed class ExpressionBuilderProjectionTests
{
    [Fact]
    public void Build_CountProjectionWithNonEmptyCollection_EvaluatesGreaterThan()
    {
        var predicate = BuildCountGreaterThan(1);

        Assert.True(predicate(new TestClass
        {
            SubEntities =
            [
                new TestClass(),
                new TestClass()
            ]
        }));

        Assert.False(predicate(new TestClass
        {
            SubEntities =
            [
                new TestClass()
            ]
        }));
    }

    [Fact]
    public void Build_CountProjectionWithEmptyCollection_EvaluatesFalse()
    {
        var predicate = BuildCountGreaterThan(0);

        Assert.False(predicate(new TestClass
        {
            SubEntities = []
        }));
    }

    [Fact]
    public void Build_CountProjectionWithNullCollection_EvaluatesFalse()
    {
        var predicate = BuildCountGreaterThan(0);

        var entity = new TestClass
        {
            SubEntities = null!
        };

        var ex = Record.Exception(() => predicate(entity));

        Assert.Null(ex);
        Assert.False(predicate(entity));
    }

    [Fact]
    public void Build_CountProjectionEqualZero_WithEmptyCollection_EvaluatesTrue()
    {
        var predicate = BuildCountEqualTo(0);

        Assert.True(predicate(new TestClass
        {
            SubEntities = []
        }));
    }

    private static Func<TestClass, bool> BuildCountGreaterThan(int value)
    {
        var builder = CreateBuilder();

        var expr = builder.Build(
            new ComparisonNode(
                new ProjectionNode(
                    new PropertyPathNode([nameof(TestClass.SubEntities)]),
                    CollectionProjection.Count),
                ComparisonOperator.GreaterThan,
                new NumberLiteral(value)));

        return expr.Compile();
    }

    private static Func<TestClass, bool> BuildCountEqualTo(int value)
    {
        var builder = CreateBuilder();

        var expr = builder.Build(
            new ComparisonNode(
                new ProjectionNode(
                    new PropertyPathNode([nameof(TestClass.SubEntities)]),
                    CollectionProjection.Count),
                ComparisonOperator.Equal,
                new NumberLiteral(value)));

        return expr.Compile();
    }

    private static ExpressionBuilder<TestClass> CreateBuilder() =>
        new(new ExpressionBuilderOptions
        {
            EnableNullGuards = true,
            ParameterizeValues = false
        });
}
