namespace Zift.Querying.ExpressionBuilding;

using Fixture;
using Model;

public sealed class ExpressionBuilderNullComparisonTests
{
    [Fact]
    public void Build_EqualComparisonWithNullableInt32_MatchesNull()
    {
        var predicate = Build(
            nameof(TestClass.NullableInt32Value),
            ComparisonOperator.Equal);

        Assert.True(predicate(new TestClass { NullableInt32Value = null }));
        Assert.False(predicate(new TestClass { NullableInt32Value = 5 }));
    }

    [Fact]
    public void Build_NotEqualComparisonWithNullableInt32_MatchesNonNull()
    {
        var predicate = Build(
            nameof(TestClass.NullableInt32Value),
            ComparisonOperator.NotEqual);

        Assert.True(predicate(new TestClass { NullableInt32Value = 5 }));
        Assert.False(predicate(new TestClass { NullableInt32Value = null }));
    }

    [Fact]
    public void Build_EqualComparisonWithNullableBoolean_MatchesNull()
    {
        var predicate = Build(
            nameof(TestClass.NullableBooleanValue),
            ComparisonOperator.Equal);

        Assert.True(predicate(new TestClass { NullableBooleanValue = null }));
        Assert.False(predicate(new TestClass { NullableBooleanValue = true }));
    }

    [Fact]
    public void Build_NotEqualComparisonWithNullableBoolean_MatchesNonNull()
    {
        var predicate = Build(
            nameof(TestClass.NullableBooleanValue),
            ComparisonOperator.NotEqual);

        Assert.True(predicate(new TestClass { NullableBooleanValue = false }));
        Assert.False(predicate(new TestClass { NullableBooleanValue = null }));
    }

    [Fact]
    public void Build_EqualComparisonWithNullableGuid_MatchesNull()
    {
        var predicate = Build(
            nameof(TestClass.NullableGuidValue),
            ComparisonOperator.Equal);

        Assert.True(predicate(new TestClass { NullableGuidValue = null }));
        Assert.False(predicate(new TestClass { NullableGuidValue = Guid.NewGuid() }));
    }

    [Fact]
    public void Build_NotEqualComparisonWithNullableGuid_MatchesNonNull()
    {
        var predicate = Build(
            nameof(TestClass.NullableGuidValue),
            ComparisonOperator.NotEqual);

        Assert.True(predicate(new TestClass { NullableGuidValue = Guid.NewGuid() }));
        Assert.False(predicate(new TestClass { NullableGuidValue = null }));
    }

    [Fact]
    public void Build_NullComparisonWithNonNullableProperty_ThrowsInvalidOperationException()
    {
        var builder = CreateBuilder();

        Assert.Throws<InvalidOperationException>(() =>
            builder.Build(
                new ComparisonNode(
                    new PropertyPathNode([nameof(TestClass.Int32Value)]),
                    ComparisonOperator.Equal,
                    new NullLiteral())));
    }

    [Fact]
    public void Build_NullComparisonWithGuard_Equal_AllowsNullNavigation()
    {
        var builder = new ExpressionBuilder<TestClass>(
            new ExpressionBuilderOptions
            {
                EnableNullGuards = true
            });

        var predicate = builder.Build(
            new ComparisonNode(
                new PropertyPathNode(
                [
                    nameof(TestClass.NestedEntity),
                    nameof(TestClass.NullableInt32Value)
                ]),
                ComparisonOperator.Equal,
                new NullLiteral()))
            .Compile();

        Assert.True(predicate(new TestClass
        {
            NestedEntity = null
        }));

        Assert.True(predicate(new TestClass
        {
            NestedEntity = new TestClass
            {
                NullableInt32Value = null
            }
        }));

        Assert.False(predicate(new TestClass
        {
            NestedEntity = new TestClass
            {
                NullableInt32Value = 5
            }
        }));
    }

    [Fact]
    public void Build_NullComparisonWithGuard_NotEqual_RequiresNonNullNavigation()
    {
        var builder = new ExpressionBuilder<TestClass>(
            new ExpressionBuilderOptions
            {
                EnableNullGuards = true
            });

        var predicate = builder.Build(
            new ComparisonNode(
                new PropertyPathNode(
                [
                    nameof(TestClass.NestedEntity),
                    nameof(TestClass.NullableInt32Value)
                ]),
                ComparisonOperator.NotEqual,
                new NullLiteral()))
            .Compile();

        Assert.False(predicate(new TestClass
        {
            NestedEntity = null
        }));

        Assert.False(predicate(new TestClass
        {
            NestedEntity = new TestClass
            {
                NullableInt32Value = null
            }
        }));

        Assert.True(predicate(new TestClass
        {
            NestedEntity = new TestClass
            {
                NullableInt32Value = 5
            }
        }));
    }

    private static Func<TestClass, bool> Build(
        string propertyName,
        ComparisonOperator op)
    {
        var builder = CreateBuilder();

        var expr = builder.Build(
            new ComparisonNode(
                new PropertyPathNode([propertyName]),
                op,
                new NullLiteral()));

        return expr.Compile();
    }

    private static ExpressionBuilder<TestClass> CreateBuilder() =>
        new(new ExpressionBuilderOptions
        {
            EnableNullGuards = false,
            ParameterizeValues = false
        });
}
