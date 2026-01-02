namespace Zift.Querying.ExpressionBuilding;

using Fixture;
using Model;

public sealed class ExpressionBuilderNullGuardTests
{
    [Fact]
    public void Build_AnyQuantifierWithNullCollection_AllowsNullCollection()
    {
        var predicate = BuildAnyInt32GreaterThan(3);

        var entity = new TestClass
        {
            SubEntities = null!
        };

        var ex = Record.Exception(() => predicate(entity));

        Assert.Null(ex);
    }

    [Fact]
    public void Build_AllQuantifierWithNullCollection_AllowsNullCollection()
    {
        var predicate = BuildAllInt32GreaterThan(3);

        var entity = new TestClass
        {
            SubEntities = null!
        };

        var ex = Record.Exception(() => predicate(entity));

        Assert.Null(ex);
    }

    [Fact]
    public void Build_AnyQuantifierWithNullElements_IgnoresNullElements()
    {
        var predicate = BuildAnyInt32GreaterThan(3);

        var entity = new TestClass
        {
            SubEntities =
            [
                null!,
                new TestClass { Int32Value = 4 }
            ]
        };

        var ex = Record.Exception(() => predicate(entity));

        Assert.Null(ex);
        Assert.True(predicate(entity));
    }

    private static Func<TestClass, bool> BuildAnyInt32GreaterThan(int threshold)
    {
        var builder = CreateBuilder();

        return builder.Build(
            new QuantifierNode(
                new PropertyPathNode([nameof(TestClass.SubEntities)]),
                QuantifierKind.Any,
                new ComparisonNode(
                    new PropertyPathNode([nameof(TestClass.Int32Value)]),
                    ComparisonOperator.GreaterThan,
                    new NumberLiteral(threshold))))
            .Compile();
    }

    private static Func<TestClass, bool> BuildAllInt32GreaterThan(int threshold)
    {
        var builder = CreateBuilder();

        return builder.Build(
            new QuantifierNode(
                new PropertyPathNode([nameof(TestClass.SubEntities)]),
                QuantifierKind.All,
                new ComparisonNode(
                    new PropertyPathNode([nameof(TestClass.Int32Value)]),
                    ComparisonOperator.GreaterThan,
                    new NumberLiteral(threshold))))
            .Compile();
    }

    private static ExpressionBuilder<TestClass> CreateBuilder() =>
        new(new ExpressionBuilderOptions
        {
            EnableNullGuards = true,
            ParameterizeValues = false
        });
}
