namespace Zift.Querying.ExpressionBuilding;

using Fixture;
using Model;

public sealed class ExpressionBuilderQuantifierTests
{
    [Fact]
    public void Build_AnyQuantifierWithoutPredicate_WithNonEmptyCollection_ReturnsTrue()
    {
        var predicate = BuildAnyWithoutPredicate();

        Assert.True(predicate(new TestClass
        {
            SubEntities = [new TestClass()]
        }));

        Assert.False(predicate(new TestClass
        {
            SubEntities = []
        }));
    }

    [Fact]
    public void Build_AnyQuantifierWithPredicate_WhenAnyElementMatches_ReturnsTrue()
    {
        var predicate = BuildAnyInt32GreaterThan(2);

        Assert.True(predicate(new TestClass
        {
            SubEntities =
            [
                new TestClass { Int32Value = 1 },
                new TestClass { Int32Value = 3 }
            ]
        }));

        Assert.False(predicate(new TestClass
        {
            SubEntities =
            [
                new TestClass { Int32Value = 1 },
                new TestClass { Int32Value = 2 }
            ]
        }));
    }

    [Fact]
    public void Build_AllQuantifierWithPredicate_WhenAllElementsMatch_ReturnsTrue()
    {
        var predicate = BuildAllInt32GreaterThan(2);

        Assert.True(predicate(new TestClass
        {
            SubEntities =
            [
                new TestClass { Int32Value = 3 },
                new TestClass { Int32Value = 4 }
            ]
        }));

        Assert.False(predicate(new TestClass
        {
            SubEntities =
            [
                new TestClass { Int32Value = 1 },
                new TestClass { Int32Value = 3 }
            ]
        }));
    }

    [Fact]
    public void Build_AllQuantifierWithPredicate_WithEmptyCollection_ReturnsTrue()
    {
        var predicate = BuildAllInt32GreaterThan(2);

        Assert.True(predicate(new TestClass
        {
            SubEntities = []
        }));
    }

    [Fact]
    public void Build_AllQuantifierWithNullCollection_ReturnsTrue()
    {
        var predicate = BuildAllInt32GreaterThan(2);

        Assert.True(predicate(new TestClass
        {
            SubEntities = null!
        }));
    }

    [Fact]
    public void Build_AnyQuantifierWithNullCollection_ReturnsFalse()
    {
        var predicate = BuildAnyInt32GreaterThan(2);

        Assert.False(predicate(new TestClass
        {
            SubEntities = null!
        }));
    }

    private static Func<TestClass, bool> BuildAnyWithoutPredicate()
    {
        var builder = CreateBuilder();

        return builder.Build(
            new QuantifierNode(
                new PropertyPathNode([nameof(TestClass.SubEntities)]),
                QuantifierKind.Any,
                Predicate: null))
            .Compile();
    }

    private static Func<TestClass, bool> BuildAnyInt32GreaterThan(int value)
    {
        var builder = CreateBuilder();

        return builder.Build(
            new QuantifierNode(
                new PropertyPathNode([nameof(TestClass.SubEntities)]),
                QuantifierKind.Any,
                new ComparisonNode(
                    new PropertyPathNode([nameof(TestClass.Int32Value)]),
                    ComparisonOperator.GreaterThan,
                    new NumberLiteral(value))))
            .Compile();
    }

    private static Func<TestClass, bool> BuildAllInt32GreaterThan(int value)
    {
        var builder = CreateBuilder();

        return builder.Build(
            new QuantifierNode(
                new PropertyPathNode([nameof(TestClass.SubEntities)]),
                QuantifierKind.All,
                new ComparisonNode(
                    new PropertyPathNode([nameof(TestClass.Int32Value)]),
                    ComparisonOperator.GreaterThan,
                    new NumberLiteral(value))))
            .Compile();
    }

    [Fact]
    public void Build_AnyQuantifier_WhenNullGuardsDisabled_UsesUnguardedMethodCall()
    {
        var builder = new ExpressionBuilder<TestClass>(
            new ExpressionBuilderOptions
            {
                EnableNullGuards = false
            });

        var predicate = builder.Build(
            new QuantifierNode(
                new PropertyPathNode([nameof(TestClass.SubEntities)]),
                QuantifierKind.Any,
                new ComparisonNode(
                    new PropertyPathNode([nameof(TestClass.Int32Value)]),
                    ComparisonOperator.GreaterThan,
                    new NumberLiteral(3))))
            .Compile();

        Assert.True(predicate(new TestClass
        {
            SubEntities =
            [
                new TestClass { Int32Value = 5 }
            ]
        }));

        Assert.False(predicate(new TestClass
        {
            SubEntities =
            [
                new TestClass { Int32Value = 1 }
            ]
        }));
    }

    private static ExpressionBuilder<TestClass> CreateBuilder() =>
        new(new ExpressionBuilderOptions
        {
            EnableNullGuards = true,
            ParameterizeValues = false
        });
}
